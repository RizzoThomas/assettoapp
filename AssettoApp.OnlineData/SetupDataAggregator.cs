using AssettoApp.Core.Interfaces;
using AssettoApp.Core.Models;
using AssettoApp.Core.Repositories;

namespace AssettoApp.OnlineData;

/// <summary>
/// Aggregates setup data from multiple sources with fallback logic
/// </summary>
public class SetupDataAggregator
{
    private readonly List<IOnlineDataProvider> _onlineProviders;
    private readonly PresetDataRepository _presetRepository;
    private readonly ISetupGenerator _setupGenerator;
    
    public SetupDataAggregator(
        IEnumerable<IOnlineDataProvider> onlineProviders,
        PresetDataRepository presetRepository,
        ISetupGenerator setupGenerator)
    {
        _onlineProviders = onlineProviders.ToList();
        _presetRepository = presetRepository;
        _setupGenerator = setupGenerator;
    }
    
    /// <summary>
    /// Generate setup using all available data sources with confidence scoring
    /// </summary>
    public async Task<(CarSetup Setup, SetupConfidence Confidence)> GenerateSetupWithConfidenceAsync(
        GameType gameType,
        string carName,
        string trackName,
        string drivingStyle,
        CancellationToken cancellationToken = default)
    {
        var confidence = new SetupConfidence
        {
            SourceContributions = new Dictionary<DataSourceType, float>()
        };
        
        // Try to get online data
        var onlineSetups = new List<OnlineSetupData>();
        foreach (var provider in _onlineProviders)
        {
            try
            {
                if (await provider.IsAvailableAsync())
                {
                    var setups = await provider.SearchSetupsAsync(gameType, carName, trackName, cancellationToken);
                    onlineSetups.AddRange(setups);
                    confidence.SourceContributions[DataSourceType.OnlineDatabase] = 0.4f;
                }
            }
            catch (Exception)
            {
                // Provider failed, continue with others
                confidence.MissingDataSources.Add($"{provider.ProviderName} unavailable");
            }
        }
        
        CarSetup finalSetup;
        
        if (onlineSetups.Any())
        {
            // Use online data as primary source
            var bestSetup = onlineSetups.OrderByDescending(s => s.ConfidenceScore).First();
            finalSetup = bestSetup.Setup;
            confidence.OverallScore = bestSetup.ConfidenceScore;
            confidence.RecommendationQuality = bestSetup.ConfidenceScore > 0.7f ? "High" : "Medium";
        }
        else
        {
            // Fallback to local preset generation
            finalSetup = _setupGenerator.GenerateOfflineSetup(carName, trackName, gameType, drivingStyle);
            confidence.SourceContributions[DataSourceType.LocalPreset] = 0.6f;
            confidence.OverallScore = 0.5f; // Medium confidence for preset-only
            confidence.RecommendationQuality = "Medium";
            confidence.MissingDataSources.Add("No online data available");
        }
        
        return (finalSetup, confidence);
    }
    
    /// <summary>
    /// Check if any online providers are available
    /// </summary>
    public async Task<bool> IsOnlineDataAvailableAsync()
    {
        foreach (var provider in _onlineProviders)
        {
            if (await provider.IsAvailableAsync())
            {
                return true;
            }
        }
        return false;
    }
}

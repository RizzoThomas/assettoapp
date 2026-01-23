using AssettoApp.Core.Interfaces;
using AssettoApp.Core.Models;

namespace AssettoApp.OnlineData.Providers;

/// <summary>
/// Provider for Setup Market database
/// NOTE: This is a framework implementation. Actual API integration requires:
/// - Setup Market API endpoints
/// - Authentication mechanism
/// - Data format specifications
/// </summary>
public class SetupMarketProvider : OnlineDataProviderBase
{
    public override string ProviderName => "Setup Market";
    
    public SetupMarketProvider(HttpClient httpClient) 
        : base(httpClient, "https://setupmarket.io")
    {
    }
    
    public override async Task<List<OnlineSetupData>> SearchSetupsAsync(
        GameType gameType,
        string carName,
        string trackName,
        CancellationToken cancellationToken = default)
    {
        // Setup Market does not provide a public API
        // This implementation provides a framework for future integration
        // when official API becomes available or with user-configured scraping
        
        // For now, return empty list - users should configure via ConfigurableScrapingProvider
        // See SetupSourceConfigManager for example configurations
        
        await Task.Delay(100, cancellationToken);
        return new List<OnlineSetupData>();
    }
    
    public override async Task<OnlineSetupData?> GetCommunityAverageAsync(
        GameType gameType,
        string carName,
        string trackName,
        CancellationToken cancellationToken = default)
    {
        // Setup Market does not provide community average data via public API
        // Use ConfigurableScrapingProvider for actual data extraction
        
        await Task.Delay(100, cancellationToken);
        return null;
    }
    
    public override async Task<TrackCharacteristics?> GetTrackDataAsync(
        string trackName,
        CancellationToken cancellationToken = default)
    {
        // Track data not available via public API
        // Falls back to local PresetDataRepository
        
        await Task.Delay(100, cancellationToken);
        return null;
    }
    
    public override async Task<CarTechnicalSpecs?> GetCarSpecsAsync(
        GameType gameType,
        string carName,
        CancellationToken cancellationToken = default)
    {
        // Car specifications not available via public API
        // Falls back to local PresetDataRepository
        
        await Task.Delay(100, cancellationToken);
        return null;
    }
}

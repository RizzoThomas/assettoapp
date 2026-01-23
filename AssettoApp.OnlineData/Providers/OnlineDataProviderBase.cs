using AssettoApp.Core.Interfaces;
using AssettoApp.Core.Models;

namespace AssettoApp.OnlineData.Providers;

/// <summary>
/// Base class for online data providers with common functionality
/// </summary>
public abstract class OnlineDataProviderBase : IOnlineDataProvider
{
    protected readonly HttpClient _httpClient;
    protected readonly string _baseUrl;
    
    public abstract string ProviderName { get; }
    
    protected OnlineDataProviderBase(HttpClient httpClient, string baseUrl)
    {
        _httpClient = httpClient;
        _baseUrl = baseUrl;
    }
    
    public virtual async Task<bool> IsAvailableAsync()
    {
        try
        {
            // Check internet connectivity and API availability
            var response = await _httpClient.GetAsync(_baseUrl, HttpCompletionOption.ResponseHeadersRead);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
    
    public abstract Task<List<OnlineSetupData>> SearchSetupsAsync(
        GameType gameType,
        string carName,
        string trackName,
        CancellationToken cancellationToken = default);
    
    public abstract Task<OnlineSetupData?> GetCommunityAverageAsync(
        GameType gameType,
        string carName,
        string trackName,
        CancellationToken cancellationToken = default);
    
    public abstract Task<TrackCharacteristics?> GetTrackDataAsync(
        string trackName,
        CancellationToken cancellationToken = default);
    
    public abstract Task<CarTechnicalSpecs?> GetCarSpecsAsync(
        GameType gameType,
        string carName,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Helper to calculate confidence score based on metadata
    /// </summary>
    protected float CalculateConfidenceScore(OnlineSetupMetadata metadata)
    {
        float score = 0.5f; // Base score
        
        // Boost for high ratings
        if (metadata.RatingCount > 0)
        {
            score += (metadata.Rating / 5.0f) * 0.3f;
        }
        
        // Boost for popularity
        if (metadata.DownloadCount > 100)
        {
            score += 0.1f;
        }
        else if (metadata.DownloadCount > 10)
        {
            score += 0.05f;
        }
        
        // Reduce for old setups (> 1 year)
        var age = DateTime.UtcNow - metadata.CreatedDate;
        if (age.TotalDays > 365)
        {
            score -= 0.1f;
        }
        
        return Math.Clamp(score, 0.0f, 1.0f);
    }
}

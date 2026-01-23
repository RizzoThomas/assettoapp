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
        // TODO: Implement Setup Market API integration
        
        await Task.Delay(100, cancellationToken);
        return new List<OnlineSetupData>();
    }
    
    public override async Task<OnlineSetupData?> GetCommunityAverageAsync(
        GameType gameType,
        string carName,
        string trackName,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement community average from Setup Market
        
        await Task.Delay(100, cancellationToken);
        return null;
    }
    
    public override async Task<TrackCharacteristics?> GetTrackDataAsync(
        string trackName,
        CancellationToken cancellationToken = default)
    {
        // TODO: Fetch track characteristics
        
        await Task.Delay(100, cancellationToken);
        return null;
    }
    
    public override async Task<CarTechnicalSpecs?> GetCarSpecsAsync(
        GameType gameType,
        string carName,
        CancellationToken cancellationToken = default)
    {
        // TODO: Fetch car specs
        
        await Task.Delay(100, cancellationToken);
        return null;
    }
}

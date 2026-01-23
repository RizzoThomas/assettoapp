using AssettoApp.Core.Models;

namespace AssettoApp.Core.Interfaces;

/// <summary>
/// Interface for online data providers (RaceDepartment, Setup Market, etc.)
/// </summary>
public interface IOnlineDataProvider
{
    /// <summary>
    /// Name of the data provider
    /// </summary>
    string ProviderName { get; }
    
    /// <summary>
    /// Check if the provider is currently available (internet connection, API status)
    /// </summary>
    Task<bool> IsAvailableAsync();
    
    /// <summary>
    /// Search for setups matching the criteria
    /// </summary>
    Task<List<OnlineSetupData>> SearchSetupsAsync(
        GameType gameType,
        string carName,
        string trackName,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get community average setup for a car/track combination
    /// </summary>
    Task<OnlineSetupData?> GetCommunityAverageAsync(
        GameType gameType,
        string carName,
        string trackName,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get track characteristics from online sources
    /// </summary>
    Task<TrackCharacteristics?> GetTrackDataAsync(
        string trackName,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get car technical specifications from online sources
    /// </summary>
    Task<CarTechnicalSpecs?> GetCarSpecsAsync(
        GameType gameType,
        string carName,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Track characteristics from online sources
/// </summary>
public class TrackCharacteristics
{
    public string TrackName { get; set; } = string.Empty;
    public float LengthKm { get; set; }
    public int CornerCount { get; set; }
    public List<TrackSector> Sectors { get; set; } = new();
    public float AverageSpeed { get; set; }
    public float TopSpeed { get; set; }
    public float ElevationChange { get; set; }
    public string SurfaceType { get; set; } = string.Empty;
    public Dictionary<string, float> SpeedProfile { get; set; } = new(); // Position -> Speed
}

/// <summary>
/// Track sector information
/// </summary>
public class TrackSector
{
    public int SectorNumber { get; set; }
    public float LengthKm { get; set; }
    public string CharacterType { get; set; } = string.Empty; // High-speed, Technical, Mixed
    public int CornerCount { get; set; }
}

/// <summary>
/// Car technical specifications from online sources
/// </summary>
public class CarTechnicalSpecs
{
    public string CarName { get; set; } = string.Empty;
    public GameType GameType { get; set; }
    public int PowerHP { get; set; }
    public int WeightKg { get; set; }
    public string DrivetrainLayout { get; set; } = string.Empty; // RWD, FWD, AWD
    public string EngineLayout { get; set; } = string.Empty; // Front, Mid, Rear
    public float WeightDistribution { get; set; } // 0.0 to 1.0 (front bias)
    public Dictionary<string, float> SetupRanges { get; set; } = new(); // Parameter -> Max value
}

namespace AssettoApp.Core.Models;

/// <summary>
/// Represents setup data fetched from online sources
/// </summary>
public class OnlineSetupData
{
    public string Source { get; set; } = string.Empty;
    public string CarName { get; set; } = string.Empty;
    public string TrackName { get; set; } = string.Empty;
    public GameType GameType { get; set; }
    public DateTime FetchedAt { get; set; }
    public float ConfidenceScore { get; set; } // 0.0 to 1.0
    
    /// <summary>
    /// The actual setup parameters
    /// </summary>
    public CarSetup Setup { get; set; } = new();
    
    /// <summary>
    /// Metadata about the setup
    /// </summary>
    public OnlineSetupMetadata Metadata { get; set; } = new();
}

/// <summary>
/// Metadata about online setup data
/// </summary>
public class OnlineSetupMetadata
{
    public string Author { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public int DownloadCount { get; set; }
    public float Rating { get; set; } // 0.0 to 5.0
    public int RatingCount { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public string Version { get; set; } = string.Empty;
}

/// <summary>
/// Data source type for tracking where data comes from
/// </summary>
public enum DataSourceType
{
    LocalPreset,
    LocalCache,
    UserProfile,
    PreviousSession,
    OnlineDatabase,
    CommunityAverage,
    ExpertRecommendation
}

/// <summary>
/// Confidence level for setup recommendations
/// </summary>
public class SetupConfidence
{
    public float OverallScore { get; set; } // 0.0 to 1.0
    public Dictionary<DataSourceType, float> SourceContributions { get; set; } = new();
    public List<string> MissingDataSources { get; set; } = new();
    public string RecommendationQuality { get; set; } = string.Empty; // Low, Medium, High, Expert
}

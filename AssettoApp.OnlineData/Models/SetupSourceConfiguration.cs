namespace AssettoApp.OnlineData.Models;

/// <summary>
/// Configuration for online setup data sources
/// </summary>
public class SetupSourceConfiguration
{
    /// <summary>
    /// Name of the data source
    /// </summary>
    public string SourceName { get; set; } = string.Empty;
    
    /// <summary>
    /// Base URL for the source
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// URL pattern for searching setups
    /// Placeholders: {game}, {car}, {track}
    /// Example: "https://example.com/setups?game={game}&car={car}&track={track}"
    /// </summary>
    public string SearchUrlPattern { get; set; } = string.Empty;
    
    /// <summary>
    /// CSS selector or XPath for extracting setup items from search results
    /// </summary>
    public string SetupItemSelector { get; set; } = string.Empty;
    
    /// <summary>
    /// CSS selector for setup title/name
    /// </summary>
    public string TitleSelector { get; set; } = string.Empty;
    
    /// <summary>
    /// CSS selector for author name
    /// </summary>
    public string AuthorSelector { get; set; } = string.Empty;
    
    /// <summary>
    /// CSS selector for download link
    /// </summary>
    public string DownloadLinkSelector { get; set; } = string.Empty;
    
    /// <summary>
    /// CSS selector for description/notes
    /// </summary>
    public string DescriptionSelector { get; set; } = string.Empty;
    
    /// <summary>
    /// CSS selector for rating
    /// </summary>
    public string RatingSelector { get; set; } = string.Empty;
    
    /// <summary>
    /// CSS selector for download count
    /// </summary>
    public string DownloadCountSelector { get; set; } = string.Empty;
    
    /// <summary>
    /// Minimum delay between requests in milliseconds (rate limiting)
    /// </summary>
    public int MinRequestDelayMs { get; set; } = 1000;
    
    /// <summary>
    /// Maximum number of requests per minute
    /// </summary>
    public int MaxRequestsPerMinute { get; set; } = 30;
    
    /// <summary>
    /// Whether this source requires authentication
    /// </summary>
    public bool RequiresAuthentication { get; set; }
    
    /// <summary>
    /// API key if required
    /// </summary>
    public string? ApiKey { get; set; }
    
    /// <summary>
    /// Whether the source is currently enabled
    /// </summary>
    public bool IsEnabled { get; set; } = true;
}

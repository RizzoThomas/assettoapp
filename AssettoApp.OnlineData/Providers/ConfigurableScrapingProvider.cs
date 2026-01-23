using AssettoApp.Core.Interfaces;
using AssettoApp.Core.Models;
using AssettoApp.OnlineData.Models;
using AssettoApp.OnlineData.Utilities;
using HtmlAgilityPack;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Web;

namespace AssettoApp.OnlineData.Providers;

/// <summary>
/// Configurable provider that can scrape setup data from various websites
/// based on user-provided configuration
/// </summary>
public class ConfigurableScrapingProvider : OnlineDataProviderBase
{
    private readonly SetupSourceConfiguration _config;
    private readonly RateLimiter _rateLimiter;
    private readonly HtmlWeb _htmlWeb;

    public override string ProviderName => _config.SourceName;

    public ConfigurableScrapingProvider(
        HttpClient httpClient, 
        SetupSourceConfiguration config) 
        : base(httpClient, config.BaseUrl)
    {
        _config = config;
        _rateLimiter = new RateLimiter(config.MinRequestDelayMs, config.MaxRequestsPerMinute);
        _htmlWeb = new HtmlWeb();
    }

    public override async Task<bool> IsAvailableAsync()
    {
        if (!_config.IsEnabled || string.IsNullOrEmpty(_config.BaseUrl))
            return false;

        return await base.IsAvailableAsync();
    }

    public override async Task<List<OnlineSetupData>> SearchSetupsAsync(
        GameType gameType,
        string carName,
        string trackName,
        CancellationToken cancellationToken = default)
    {
        if (!_config.IsEnabled || string.IsNullOrEmpty(_config.SearchUrlPattern))
            return new List<OnlineSetupData>();

        try
        {
            // Wait for rate limiting
            await _rateLimiter.WaitIfNeededAsync(cancellationToken);

            // Build search URL
            var searchUrl = BuildSearchUrl(gameType, carName, trackName);
            
            // Fetch the page
            var html = await FetchHtmlAsync(searchUrl, cancellationToken);
            if (html == null)
                return new List<OnlineSetupData>();

            // Parse the page
            var setups = ParseSetupsFromHtml(html, gameType, carName, trackName);
            
            return setups;
        }
        catch (Exception)
        {
            // Return empty list on error - fallback to other sources
            return new List<OnlineSetupData>();
        }
    }

    public override async Task<OnlineSetupData?> GetCommunityAverageAsync(
        GameType gameType,
        string carName,
        string trackName,
        CancellationToken cancellationToken = default)
    {
        // Get all setups and compute average
        var setups = await SearchSetupsAsync(gameType, carName, trackName, cancellationToken);
        
        if (setups.Count == 0)
            return null;

        // For now, return the highest rated setup as a proxy for community average
        // In a real implementation, we would average the actual setup values
        var bestSetup = setups
            .OrderByDescending(s => s.Metadata.Rating)
            .ThenByDescending(s => s.Metadata.DownloadCount)
            .FirstOrDefault();

        if (bestSetup != null)
        {
            bestSetup.Source = $"{ProviderName} (Community Average)";
            bestSetup.Metadata.Description = "Based on highest-rated community setup";
        }

        return bestSetup;
    }

    public override Task<TrackCharacteristics?> GetTrackDataAsync(
        string trackName,
        CancellationToken cancellationToken = default)
    {
        // Track data scraping would require additional configuration
        // For now, return null to use local data
        return Task.FromResult<TrackCharacteristics?>(null);
    }

    public override Task<CarTechnicalSpecs?> GetCarSpecsAsync(
        GameType gameType,
        string carName,
        CancellationToken cancellationToken = default)
    {
        // Car specs scraping would require additional configuration
        // For now, return null to use local data
        return Task.FromResult<CarTechnicalSpecs?>(null);
    }

    private string BuildSearchUrl(GameType gameType, string carName, string trackName)
    {
        var url = _config.SearchUrlPattern
            .Replace("{game}", EncodeUrlParameter(GameTypeToString(gameType)))
            .Replace("{car}", EncodeUrlParameter(carName))
            .Replace("{track}", EncodeUrlParameter(trackName));

        return url;
    }

    private string GameTypeToString(GameType gameType)
    {
        return gameType switch
        {
            GameType.AssettoCorsaOriginal => "assetto-corsa",
            GameType.AssettoCorساEvo => "assetto-corsa-evo",
            _ => "assetto-corsa"
        };
    }

    private string EncodeUrlParameter(string param)
    {
        return Uri.EscapeDataString(param);
    }

    private async Task<HtmlDocument?> FetchHtmlAsync(string url, CancellationToken cancellationToken)
    {
        try
        {
            // Use HtmlAgilityPack to load the page
            var doc = await Task.Run(() => _htmlWeb.Load(url), cancellationToken);
            return doc;
        }
        catch
        {
            return null;
        }
    }

    private List<OnlineSetupData> ParseSetupsFromHtml(
        HtmlDocument html, 
        GameType gameType, 
        string carName, 
        string trackName)
    {
        var setups = new List<OnlineSetupData>();

        if (string.IsNullOrEmpty(_config.SetupItemSelector))
            return setups;

        try
        {
            // Find all setup items
            var itemNodes = html.DocumentNode.SelectNodes(_config.SetupItemSelector);
            if (itemNodes == null || itemNodes.Count == 0)
                return setups;

            foreach (var itemNode in itemNodes)
            {
                try
                {
                    var setup = ParseSetupItem(itemNode, gameType, carName, trackName);
                    if (setup != null)
                    {
                        setups.Add(setup);
                    }
                }
                catch
                {
                    // Skip this item if parsing fails
                    continue;
                }
            }
        }
        catch
        {
            // Return whatever we parsed successfully
        }

        return setups;
    }

    private OnlineSetupData? ParseSetupItem(
        HtmlNode itemNode, 
        GameType gameType, 
        string carName, 
        string trackName)
    {
        var metadata = new OnlineSetupMetadata();

        // Extract title
        if (!string.IsNullOrEmpty(_config.TitleSelector))
        {
            var titleNode = itemNode.SelectSingleNode(_config.TitleSelector);
            metadata.Description = titleNode?.InnerText?.Trim() ?? "Setup";
        }

        // Extract author
        if (!string.IsNullOrEmpty(_config.AuthorSelector))
        {
            var authorNode = itemNode.SelectSingleNode(_config.AuthorSelector);
            metadata.Author = authorNode?.InnerText?.Trim() ?? "Unknown";
        }

        // Extract download link
        string downloadLink = string.Empty;
        if (!string.IsNullOrEmpty(_config.DownloadLinkSelector))
        {
            var linkNode = itemNode.SelectSingleNode(_config.DownloadLinkSelector);
            downloadLink = linkNode?.GetAttributeValue("href", string.Empty) ?? string.Empty;
            
            // Make absolute URL if relative
            if (!string.IsNullOrEmpty(downloadLink) && !downloadLink.StartsWith("http"))
            {
                downloadLink = new Uri(new Uri(_config.BaseUrl), downloadLink).ToString();
            }
        }

        // Extract rating
        if (!string.IsNullOrEmpty(_config.RatingSelector))
        {
            var ratingNode = itemNode.SelectSingleNode(_config.RatingSelector);
            var ratingText = ratingNode?.InnerText?.Trim() ?? "0";
            if (float.TryParse(ExtractNumber(ratingText), out var rating))
            {
                metadata.Rating = Math.Clamp(rating, 0f, 5f);
            }
        }

        // Extract download count
        if (!string.IsNullOrEmpty(_config.DownloadCountSelector))
        {
            var countNode = itemNode.SelectSingleNode(_config.DownloadCountSelector);
            var countText = countNode?.InnerText?.Trim() ?? "0";
            if (int.TryParse(ExtractNumber(countText), out var count))
            {
                metadata.DownloadCount = count;
            }
        }

        // Extract description/notes
        if (!string.IsNullOrEmpty(_config.DescriptionSelector))
        {
            var descNode = itemNode.SelectSingleNode(_config.DescriptionSelector);
            var description = descNode?.InnerText?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(description))
            {
                metadata.Description += $" - {description}";
            }
        }

        // Add download link to description
        if (!string.IsNullOrEmpty(downloadLink))
        {
            metadata.Description += $"\nDownload: {downloadLink}";
            metadata.Tags.Add("download-available");
        }

        // Set default values
        metadata.CreatedDate = DateTime.UtcNow.AddDays(-30); // Assume recent
        metadata.Version = "1.0";

        // Create the setup data
        var setupData = new OnlineSetupData
        {
            Source = ProviderName,
            CarName = carName,
            TrackName = trackName,
            GameType = gameType,
            FetchedAt = DateTime.UtcNow,
            Metadata = metadata,
            Setup = new CarSetup
            {
                CarName = carName,
                TrackName = trackName,
                GameType = gameType
            }
        };

        // Calculate confidence score
        setupData.ConfidenceScore = CalculateConfidenceScore(metadata);

        return setupData;
    }

    private string ExtractNumber(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "0";

        // Extract first number from text
        var match = Regex.Match(text, @"[\d.]+");
        return match.Success ? match.Value : "0";
    }
}

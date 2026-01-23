using AssettoApp.OnlineData.Models;
using System.Text.Json;

namespace AssettoApp.OnlineData.Utilities;

/// <summary>
/// Manages setup source configurations
/// </summary>
public class SetupSourceConfigManager
{
    private readonly string _configFilePath;
    private List<SetupSourceConfiguration> _configurations = new();

    public SetupSourceConfigManager(string? configFilePath = null)
    {
        _configFilePath = configFilePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AssettoApp",
            "setup_sources.json");

        EnsureConfigDirectoryExists();
        LoadConfigurations();
    }

    /// <summary>
    /// Get all configured sources
    /// </summary>
    public List<SetupSourceConfiguration> GetAllConfigurations()
    {
        return new List<SetupSourceConfiguration>(_configurations);
    }

    /// <summary>
    /// Get enabled sources only
    /// </summary>
    public List<SetupSourceConfiguration> GetEnabledConfigurations()
    {
        return _configurations.Where(c => c.IsEnabled).ToList();
    }

    /// <summary>
    /// Add or update a configuration
    /// </summary>
    public void SaveConfiguration(SetupSourceConfiguration config)
    {
        var existing = _configurations.FirstOrDefault(c => c.SourceName == config.SourceName);
        if (existing != null)
        {
            _configurations.Remove(existing);
        }
        
        _configurations.Add(config);
        PersistConfigurations();
    }

    /// <summary>
    /// Remove a configuration
    /// </summary>
    public void RemoveConfiguration(string sourceName)
    {
        _configurations.RemoveAll(c => c.SourceName == sourceName);
        PersistConfigurations();
    }

    /// <summary>
    /// Load example configurations for common sites
    /// </summary>
    public void LoadExampleConfigurations()
    {
        // Note: These are example configurations that may need adjustment
        // based on actual website structure
        
        var examples = new List<SetupSourceConfiguration>
        {
            new SetupSourceConfiguration
            {
                SourceName = "RaceDepartment",
                BaseUrl = "https://www.racedepartment.com",
                SearchUrlPattern = "https://www.racedepartment.com/downloads/categories/ac-setups.23/?q={car}+{track}",
                SetupItemSelector = "//div[contains(@class, 'structItem')]",
                TitleSelector = ".//h3[@class='structItem-title']//a",
                AuthorSelector = ".//a[contains(@class, 'username')]",
                DownloadLinkSelector = ".//h3[@class='structItem-title']//a/@href",
                RatingSelector = ".//span[@class='rating-value']",
                DownloadCountSelector = ".//span[contains(@class, 'download-count')]",
                MinRequestDelayMs = 2000,
                MaxRequestsPerMinute = 20,
                IsEnabled = false // Disabled by default until user verifies
            },
            new SetupSourceConfiguration
            {
                SourceName = "Setup Market",
                BaseUrl = "https://setupmarket.io",
                SearchUrlPattern = "https://setupmarket.io/setups?game=assetto-corsa&car={car}&track={track}",
                SetupItemSelector = "//div[contains(@class, 'setup-card')]",
                TitleSelector = ".//h3[@class='setup-title']",
                AuthorSelector = ".//span[@class='author-name']",
                DownloadLinkSelector = ".//a[@class='download-link']/@href",
                RatingSelector = ".//span[@class='rating']",
                DownloadCountSelector = ".//span[@class='downloads']",
                MinRequestDelayMs = 1000,
                MaxRequestsPerMinute = 30,
                IsEnabled = false // Disabled by default until user verifies
            }
        };

        foreach (var example in examples)
        {
            if (!_configurations.Any(c => c.SourceName == example.SourceName))
            {
                _configurations.Add(example);
            }
        }

        PersistConfigurations();
    }

    private void EnsureConfigDirectoryExists()
    {
        var directory = Path.GetDirectoryName(_configFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    private void LoadConfigurations()
    {
        try
        {
            if (File.Exists(_configFilePath))
            {
                var json = File.ReadAllText(_configFilePath);
                var configs = JsonSerializer.Deserialize<List<SetupSourceConfiguration>>(json);
                if (configs != null)
                {
                    _configurations = configs;
                    return;
                }
            }
        }
        catch
        {
            // If loading fails, use empty list
        }

        // If no config file exists or loading failed, load examples
        LoadExampleConfigurations();
    }

    private void PersistConfigurations()
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            var json = JsonSerializer.Serialize(_configurations, options);
            File.WriteAllText(_configFilePath, json);
        }
        catch
        {
            // Fail silently - configurations will be lost but app can continue
        }
    }

    /// <summary>
    /// Get path to configuration file
    /// </summary>
    public string GetConfigFilePath() => _configFilePath;
}

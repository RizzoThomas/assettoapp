using AssettoApp.Core.Interfaces;
using AssettoApp.OnlineData.Models;
using AssettoApp.OnlineData.Utilities;

namespace AssettoApp.OnlineData.Providers;

/// <summary>
/// Factory for creating online data providers
/// </summary>
public class OnlineDataProviderFactory
{
    private readonly HttpClient _httpClient;
    private readonly SetupSourceConfigManager _configManager;

    public OnlineDataProviderFactory(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _configManager = new SetupSourceConfigManager();
    }

    /// <summary>
    /// Get all available providers (built-in + configured)
    /// </summary>
    public List<IOnlineDataProvider> CreateAllProviders()
    {
        var providers = new List<IOnlineDataProvider>();

        // Add built-in providers (framework only, no actual functionality without configuration)
        providers.Add(new RaceDepartmentProvider(_httpClient));
        providers.Add(new SetupMarketProvider(_httpClient));

        // Add user-configured scraping providers
        var configs = _configManager.GetEnabledConfigurations();
        foreach (var config in configs)
        {
            try
            {
                var provider = new ConfigurableScrapingProvider(_httpClient, config);
                providers.Add(provider);
            }
            catch
            {
                // Skip invalid configurations
                continue;
            }
        }

        return providers;
    }

    /// <summary>
    /// Get the configuration manager
    /// </summary>
    public SetupSourceConfigManager GetConfigManager() => _configManager;

    /// <summary>
    /// Create a single provider from configuration
    /// </summary>
    public IOnlineDataProvider? CreateProvider(SetupSourceConfiguration config)
    {
        try
        {
            return new ConfigurableScrapingProvider(_httpClient, config);
        }
        catch
        {
            return null;
        }
    }
}

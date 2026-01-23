using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using AssettoApp.Core.Interfaces;
using AssettoApp.SimulatorIntegration.Connectors;
using AssettoApp.TelemetryAnalysis;
using AssettoApp.SetupGeneration;
using AssettoApp.UI.ViewModels;
using AssettoApp.Core.Models;

namespace AssettoApp.UI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Setup dependency injection
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        // Create and show main window
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Register simulatorconnectors
        services.AddSingleton<ISimulatorConnector>(sp => 
            new AssettoCorساConnector());
        
        services.AddSingleton<ISimulatorConnector>(sp => 
            new AssettoCorساEvoConnector());

        // Register analyzers and generators
        services.AddSingleton<ITelemetryAnalyzer, TelemetryAnalyzer>();
        services.AddSingleton<ISetupGenerator, SetupGenerator>();
        
        // Register lap tracking and session history
        services.AddSingleton<ILapTracker, LapTracker>();
        services.AddSingleton<ISessionHistoryRepository, AssettoApp.Core.Repositories.SessionHistoryRepository>();

        // Register ViewModels
        services.AddSingleton<MainViewModel>(sp =>
        {
            var connectors = sp.GetServices<ISimulatorConnector>().ToList();
            var acConnector = connectors.First(c => c.SupportedGame == GameType.AssettoCorsaOriginal);
            var aceConnector = connectors.First(c => c.SupportedGame == GameType.AssettoCorساEvo);
            var analyzer = sp.GetRequiredService<ITelemetryAnalyzer>();
            var generator = sp.GetRequiredService<ISetupGenerator>();
            var lapTracker = sp.GetRequiredService<ILapTracker>();
            var sessionRepository = sp.GetRequiredService<ISessionHistoryRepository>();
            
            return new MainViewModel(acConnector, aceConnector, analyzer, generator, lapTracker, sessionRepository);
        });

        // Register Views
        services.AddSingleton<MainWindow>(sp => 
            new MainWindow(sp.GetRequiredService<MainViewModel>()));
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}


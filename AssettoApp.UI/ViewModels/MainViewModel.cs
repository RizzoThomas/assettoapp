using AssettoApp.Core.Interfaces;
using AssettoApp.Core.Models;
using AssettoApp.Core.Utilities;
using AssettoApp.Core.Repositories;
using AssettoApp.UI.Commands;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;

namespace AssettoApp.UI.ViewModels;

/// <summary>
/// Main ViewModel for the AssettoApp UI
/// </summary>
public class MainViewModel : ViewModelBase
{
    private readonly ISimulatorConnector _acConnector;
    private readonly ISimulatorConnector _aceConnector;
    private readonly ITelemetryAnalyzer _telemetryAnalyzer;
    private readonly ISetupGenerator _setupGenerator;
    private readonly PresetDataRepository _presetRepository;
    private readonly ILapTracker _lapTracker;
    private readonly ISessionHistoryRepository _sessionHistoryRepository;

    private ISimulatorConnector? _currentConnector;
    private List<TelemetryData> _sessionData = new();
    private System.Threading.Timer? _telemetryTimer;

    // Properties
    private OperationMode _currentMode;
    public OperationMode CurrentMode
    {
        get => _currentMode;
        set
        {
            if (SetProperty(ref _currentMode, value))
            {
                OnPropertyChanged(nameof(IsInGameMode));
                OnPropertyChanged(nameof(IsOnlineAnalysisMode));
                UpdateUIForMode();
            }
        }
    }

    public bool IsInGameMode => CurrentMode == OperationMode.InGame;
    public bool IsOnlineAnalysisMode => CurrentMode == OperationMode.OnlineAnalysis;

    private GameType _selectedGame;
    public GameType SelectedGame
    {
        get => _selectedGame;
        set
        {
            if (SetProperty(ref _selectedGame, value))
            {
                _currentConnector = value == GameType.AssettoCorsaOriginal ? _acConnector : _aceConnector;
                _ = UpdateAvailableItems(); // Fire and forget - updates UI lists asynchronously
            }
        }
    }

    private string? _selectedCar;
    public string? SelectedCar
    {
        get => _selectedCar;
        set => SetProperty(ref _selectedCar, value);
    }

    private string? _selectedTrack;
    public string? SelectedTrack
    {
        get => _selectedTrack;
        set => SetProperty(ref _selectedTrack, value);
    }

    private string _statusMessage = "Ready";
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    private bool _isRecording;
    public bool IsRecording
    {
        get => _isRecording;
        set => SetProperty(ref _isRecording, value);
    }

    private bool _isConnected;
    public bool IsConnected
    {
        get => _isConnected;
        set => SetProperty(ref _isConnected, value);
    }

    private TelemetryAnalysisResult? _lastAnalysis;
    public TelemetryAnalysisResult? LastAnalysis
    {
        get => _lastAnalysis;
        set => SetProperty(ref _lastAnalysis, value);
    }

    // Collections
    public ObservableCollection<GameType> AvailableGames { get; }
    public ObservableCollection<string> AvailableCars { get; }
    public ObservableCollection<string> AvailableTracks { get; }
    public ObservableCollection<string> DrivingStyleOptions { get; }

    // Commands
    public ICommand SelectInGameModeCommand { get; }
    public ICommand SelectOnlineAnalysisModeCommand { get; }
    public ICommand DetectGameCommand { get; }
    public ICommand ConnectCommand { get; }
    public ICommand StartRecordingCommand { get; }
    public ICommand StopRecordingCommand { get; }
    public ICommand AnalyzeSessionCommand { get; }
    public ICommand GenerateSetupCommand { get; }
    public ICommand GenerateOnlineSetupCommand { get; }
    public ICommand ExportSetupCommand { get; }

    private string _selectedDrivingStyle = "Balanced";
    public string SelectedDrivingStyle
    {
        get => _selectedDrivingStyle;
        set => SetProperty(ref _selectedDrivingStyle, value);
    }

    private CarSetup? _lastGeneratedSetup;
    public CarSetup? LastGeneratedSetup
    {
        get => _lastGeneratedSetup;
        set => SetProperty(ref _lastGeneratedSetup, value);
    }

    // Lap tracking properties
    private int _currentLapNumber;
    public int CurrentLapNumber
    {
        get => _currentLapNumber;
        set => SetProperty(ref _currentLapNumber, value);
    }

    private TimeSpan _currentLapTime;
    public TimeSpan CurrentLapTime
    {
        get => _currentLapTime;
        set => SetProperty(ref _currentLapTime, value);
    }

    private TimeSpan? _bestLapTime;
    public TimeSpan? BestLapTime
    {
        get => _bestLapTime;
        set => SetProperty(ref _bestLapTime, value);
    }

    private int _perfectLapsCount;
    public int PerfectLapsCount
    {
        get => _perfectLapsCount;
        set => SetProperty(ref _perfectLapsCount, value);
    }

    private float _tireWearPercentage;
    public float TireWearPercentage
    {
        get => _tireWearPercentage;
        set => SetProperty(ref _tireWearPercentage, value);
    }

    private float _fuelRemaining;
    public float FuelRemaining
    {
        get => _fuelRemaining;
        set => SetProperty(ref _fuelRemaining, value);
    }

    public MainViewModel(
        ISimulatorConnector acConnector,
        ISimulatorConnector aceConnector,
        ITelemetryAnalyzer telemetryAnalyzer,
        ISetupGenerator setupGenerator,
        ILapTracker lapTracker,
        ISessionHistoryRepository sessionHistoryRepository)
    {
        _acConnector = acConnector;
        _aceConnector = aceConnector;
        _telemetryAnalyzer = telemetryAnalyzer;
        _setupGenerator = setupGenerator;
        _presetRepository = new PresetDataRepository();
        _lapTracker = lapTracker;
        _sessionHistoryRepository = sessionHistoryRepository;

        // Initialize collections
        AvailableGames = new ObservableCollection<GameType>
        {
            GameType.AssettoCorsaOriginal,
            GameType.AssettoCorساEvo
        };
        AvailableCars = new ObservableCollection<string>();
        AvailableTracks = new ObservableCollection<string>();
        DrivingStyleOptions = new ObservableCollection<string>
        {
            "Balanced",
            "Aggressive",
            "Smooth",
            "Oversteery",
            "Understeery"
        };

        // Initialize commands
        SelectInGameModeCommand = new RelayCommand(_ => SelectInGameMode());
        SelectOnlineAnalysisModeCommand = new RelayCommand(_ => SelectOnlineAnalysisMode());
        DetectGameCommand = new RelayCommand(_ => DetectRunningGame());
        ConnectCommand = new RelayCommand(_ => ConnectToSimulator(), _ => IsInGameMode && !IsConnected);
        StartRecordingCommand = new RelayCommand(_ => StartRecording(), _ => IsConnected && !IsRecording);
        StopRecordingCommand = new RelayCommand(_ => StopRecording(), _ => IsRecording);
        AnalyzeSessionCommand = new RelayCommand(_ => AnalyzeSession(), _ => _sessionData.Count > 0);
        GenerateSetupCommand = new RelayCommand(_ => GenerateSetup(), _ => LastAnalysis != null);
        GenerateOnlineSetupCommand = new RelayCommand(_ => GenerateOnlineSetup(), _ => IsOnlineAnalysisMode && !string.IsNullOrEmpty(SelectedCar) && !string.IsNullOrEmpty(SelectedTrack));
        ExportSetupCommand = new RelayCommand(_ => ExportSetup(), _ => LastGeneratedSetup != null);

        // Auto-detect game on startup
        DetectRunningGame();
    }

    private void SelectInGameMode()
    {
        CurrentMode = OperationMode.InGame;
        StatusMessage = "In-Game mode selected. Connect to running simulator to read telemetry.";
    }

    private void SelectOnlineAnalysisMode()
    {
        CurrentMode = OperationMode.OnlineAnalysis;
        StatusMessage = "Online Analysis mode selected. Using online databases + local data when game is closed.";
        LoadOfflineData();
    }

    private void DetectRunningGame()
    {
        var (isRunning, detectedGame) = ProcessDetector.DetectRunningSimulator();
        
        if (isRunning && detectedGame.HasValue)
        {
            SelectedGame = detectedGame.Value;
            CurrentMode = OperationMode.InGame;
            StatusMessage = $"{detectedGame.Value} detected running. Switched to In-Game mode.";
            
            MessageBox.Show(
                $"{detectedGame.Value} is currently running!\n\nThe application has been set to In-Game mode.\nClick 'Connect to Simulator' to start reading telemetry.",
                "Game Detected",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        else
        {
            CurrentMode = OperationMode.OnlineAnalysis;
            StatusMessage = "No simulator detected. Using Online Analysis mode with internet data sources.";
        }
    }

    private void LoadOfflineData()
    {
        // Load preset car and track lists
        AvailableCars.Clear();
        AvailableTracks.Clear();

        foreach (var car in _presetRepository.GetAvailableCars())
            AvailableCars.Add(car);

        foreach (var track in _presetRepository.GetAvailableTracks())
            AvailableTracks.Add(track);

        if (AvailableCars.Count > 0)
            SelectedCar = AvailableCars[0];

        if (AvailableTracks.Count > 0)
            SelectedTrack = AvailableTracks[0];
    }

    private void UpdateUIForMode()
    {
        if (IsOnlineAnalysisMode)
        {
            LoadOfflineData();
        }
    }

    private async void ConnectToSimulator()
    {
        if (_currentConnector == null) return;

        StatusMessage = "Connecting to simulator...";
        
        try
        {
            var connected = await _currentConnector.ConnectAsync();
            IsConnected = connected;
            
            if (connected)
            {
                StatusMessage = $"Connected to {SelectedGame}";
                await UpdateAvailableItems();
            }
            else
            {
                StatusMessage = "Failed to connect. Is the simulator running?";
                MessageBox.Show(
                    "Could not connect to the simulator. Please ensure it is running and try again.",
                    "Connection Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            IsConnected = false;
        }
    }

    private async Task UpdateAvailableItems()
    {
        if (_currentConnector == null) return;

        try
        {
            var cars = await _currentConnector.GetAvailableCarsAsync();
            var tracks = await _currentConnector.GetAvailableTracksAsync();

            AvailableCars.Clear();
            AvailableTracks.Clear();

            foreach (var car in cars)
                AvailableCars.Add(car);

            foreach (var track in tracks)
                AvailableTracks.Add(track);

            if (AvailableCars.Count > 0)
                SelectedCar = AvailableCars[0];

            if (AvailableTracks.Count > 0)
                SelectedTrack = AvailableTracks[0];
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading lists: {ex.Message}";
        }
    }

    private void StartRecording()
    {
        _sessionData.Clear();
        IsRecording = true;
        StatusMessage = "Recording telemetry...";
        
        // Start lap tracking session
        if (!string.IsNullOrEmpty(SelectedCar) && !string.IsNullOrEmpty(SelectedTrack))
        {
            _lapTracker.StartSession(SelectedGame, SelectedCar, SelectedTrack);
        }

        // Start timer to read telemetry every 100ms
        _telemetryTimer = new System.Threading.Timer(
            _ => RecordTelemetry(),
            null,
            TimeSpan.Zero,
            TimeSpan.FromMilliseconds(100));
    }

    private void StopRecording()
    {
        _telemetryTimer?.Dispose();
        _telemetryTimer = null;
        IsRecording = false;
        
        // End lap tracking and save session
        var session = _lapTracker.EndSession();
        if (session != null)
        {
            _ = SaveSessionAsync(session); // Fire and forget
            StatusMessage = $"Recording stopped. {_sessionData.Count} data points captured. {session.TotalLaps} laps completed. {session.PerfectLaps} perfect laps!";
        }
        else
        {
            StatusMessage = $"Recording stopped. {_sessionData.Count} data points captured.";
        }
    }
    
    private async Task SaveSessionAsync(SessionHistory session)
    {
        try
        {
            await _sessionHistoryRepository.SaveSessionAsync(session);
        }
        catch (Exception ex)
        {
            // Log error but don't interrupt user
            System.Diagnostics.Debug.WriteLine($"Error saving session: {ex.Message}");
        }
    }

    private void RecordTelemetry()
    {
        if (_currentConnector == null) return;

        try
        {
            var telemetry = _currentConnector.ReadTelemetry();
            if (telemetry != null)
            {
                _sessionData.Add(telemetry);
                
                // Process telemetry through lap tracker
                _lapTracker.ProcessTelemetry(telemetry);
                
                // Update UI with current lap info (marshal to UI thread)
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    CurrentLapNumber = telemetry.CurrentLap;
                    CurrentLapTime = telemetry.CurrentLapTime;
                    FuelRemaining = telemetry.Fuel;
                    
                    // Update tire wear percentage (average of all tires)
                    if (telemetry.Tires != null && telemetry.Tires.Length == 4)
                    {
                        TireWearPercentage = telemetry.Tires.Average(t => t.Wear) * 100f;
                    }
                    
                    // Update best lap and perfect laps from session
                    if (_lapTracker.CurrentSession != null)
                    {
                        BestLapTime = _lapTracker.CurrentSession.BestLapTime;
                        PerfectLapsCount = _lapTracker.CurrentSession.PerfectLaps;
                    }
                });
            }
        }
        catch
        {
            // Ignore read errors during recording
        }
    }

    private void AnalyzeSession()
    {
        if (_sessionData.Count == 0)
        {
            MessageBox.Show("No telemetry data to analyze.", "No Data", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        StatusMessage = "Analyzing session...";

        try
        {
            LastAnalysis = _telemetryAnalyzer.AnalyzeSession(_sessionData);
            StatusMessage = $"Analysis complete. Driving style: {LastAnalysis.DrivingStyle}";

            MessageBox.Show(
                $"Analysis Complete!\n\n" +
                $"Driving Style: {LastAnalysis.DrivingStyle}\n" +
                $"Average Track Temp: {LastAnalysis.TrackConditions.AverageTrackTemp:F1}°C\n" +
                $"Average Grip: {LastAnalysis.TrackConditions.AverageGrip:F2}\n" +
                $"Balance: {(LastAnalysis.BalanceAnalysis.OversteerTendency < 0 ? "Oversteer" : "Understeer")} tendency",
                "Analysis Results",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Analysis error: {ex.Message}";
            MessageBox.Show($"Error during analysis: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void GenerateSetup()
    {
        if (LastAnalysis == null || string.IsNullOrEmpty(SelectedCar) || string.IsNullOrEmpty(SelectedTrack))
        {
            MessageBox.Show("Please analyze a session first and select car/track.", "Missing Data", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        StatusMessage = "Generating optimized setup from telemetry...";

        try
        {
            var setup = _setupGenerator.GenerateSetup(LastAnalysis, SelectedCar, SelectedTrack, SelectedGame);
            LastGeneratedSetup = setup;
            StatusMessage = "Setup generated successfully from telemetry analysis!";

            MessageBox.Show(
                $"Setup Generated from Telemetry!\n\n" +
                $"Car: {setup.CarName}\n" +
                $"Track: {setup.TrackName}\n\n" +
                $"Tire Pressures:\n" +
                $"  FL: {setup.Tires.FrontLeftPressure:F1} PSI\n" +
                $"  FR: {setup.Tires.FrontRightPressure:F1} PSI\n" +
                $"  RL: {setup.Tires.RearLeftPressure:F1} PSI\n" +
                $"  RR: {setup.Tires.RearRightPressure:F1} PSI\n\n" +
                $"Suspension:\n" +
                $"  Front Spring: {setup.Suspension.FrontSpringRate:F1} N/mm\n" +
                $"  Rear Spring: {setup.Suspension.RearSpringRate:F1} N/mm\n\n" +
                $"Ready to export!",
                "Setup Generated",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Setup generation error: {ex.Message}";
            MessageBox.Show($"Error generating setup: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void GenerateOnlineSetup()
    {
        if (string.IsNullOrEmpty(SelectedCar) || string.IsNullOrEmpty(SelectedTrack))
        {
            MessageBox.Show("Please select both car and track.", "Missing Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        StatusMessage = "Generating setup using online data sources...";

        try
        {
            await GenerateOnlineSetupAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Setup generation error: {ex.Message}";
            MessageBox.Show($"Error generating setup: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task GenerateOnlineSetupAsync()
    {
        // Create HTTP client for online providers
        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(30);

        // Create aggregator with all available providers
        var aggregator = AssettoApp.OnlineData.SetupDataAggregator.CreateWithAllProviders(
            httpClient,
            _presetRepository,
            _setupGenerator);

        // Check if online data is available
        var onlineAvailable = await aggregator.IsOnlineDataAvailableAsync();
        var providerStatus = await aggregator.GetProviderStatusAsync();

        // Generate setup with confidence scoring
        var (setup, confidence) = await aggregator.GenerateSetupWithConfidenceAsync(
            SelectedGame,
            SelectedCar,
            SelectedTrack,
            SelectedDrivingStyle);
        
        LastGeneratedSetup = setup;
        StatusMessage = "Setup generated successfully!";

        // Build data sources message
        var dataSourcesText = "Data Sources:\n";
        if (onlineAvailable)
        {
            dataSourcesText += "• Online setup databases (active):\n";
            foreach (var provider in providerStatus)
            {
                var status = provider.Value ? "✓ Available" : "✗ Unavailable";
                dataSourcesText += $"  - {provider.Key}: {status}\n";
            }
        }
        else
        {
            dataSourcesText += "• Online setup databases:\n";
            dataSourcesText += "  - RaceDepartment: Framework ready (configure in setup_sources.json)\n";
            dataSourcesText += "  - Setup Market: Framework ready (configure in setup_sources.json)\n";
            dataSourcesText += "  - Custom sources: Configure in %APPDATA%\\AssettoApp\\setup_sources.json\n";
        }
        dataSourcesText += "• Local presets and physics models\n";
        dataSourcesText += "• Track characteristics database\n";

        if (confidence.MissingDataSources.Count > 0)
        {
            dataSourcesText += "\nNotes:\n";
            foreach (var note in confidence.MissingDataSources)
            {
                dataSourcesText += $"• {note}\n";
            }
        }

        MessageBox.Show(
            $"Setup Generated (Online Analysis Mode)!\n\n" +
            $"Car: {setup.CarName}\n" +
            $"Track: {setup.TrackName}\n" +
            $"Driving Style: {SelectedDrivingStyle}\n\n" +
            dataSourcesText + "\n" +
            $"Tire Pressures:\n" +
            $"  FL: {setup.Tires.FrontLeftPressure:F1} PSI\n" +
            $"  FR: {setup.Tires.FrontRightPressure:F1} PSI\n" +
            $"  RL: {setup.Tires.RearLeftPressure:F1} PSI\n" +
            $"  RR: {setup.Tires.RearRightPressure:F1} PSI\n\n" +
            $"Suspension:\n" +
            $"  Front Spring: {setup.Suspension.FrontSpringRate:F1} N/mm\n" +
            $"  Rear Spring: {setup.Suspension.RearSpringRate:F1} N/mm\n\n" +
            $"Camber:\n" +
            $"  Front: {setup.Alignment.FrontLeftCamber:F1}°\n" +
            $"  Rear: {setup.Alignment.RearLeftCamber:F1}°\n\n" +
            $"Confidence: {confidence.RecommendationQuality} ({confidence.OverallScore:P0})\n" +
            $"Ready to export!",
            "Setup Generated",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private async void ExportSetup()
    {
        if (LastGeneratedSetup == null)
        {
            MessageBox.Show("Please generate a setup first.", "No Setup", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = $"{SelectedCar}_{SelectedTrack}_setup",
                DefaultExt = ".ini",
                Filter = "Setup files (*.ini)|*.ini|All files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                var success = await _setupGenerator.ExportSetupAsync(LastGeneratedSetup, dialog.FileName);
                
                if (success)
                {
                    StatusMessage = $"Setup exported to {dialog.FileName}";
                    MessageBox.Show($"Setup exported successfully to:\n{dialog.FileName}", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    StatusMessage = "Export failed";
                    MessageBox.Show("Failed to export setup file.", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Export error: {ex.Message}";
            MessageBox.Show($"Error exporting setup: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

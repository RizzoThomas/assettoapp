using AssettoApp.Core.Interfaces;
using AssettoApp.Core.Models;
using AssettoApp.UI.Commands;
using System.Collections.ObjectModel;
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

    private ISimulatorConnector? _currentConnector;
    private List<TelemetryData> _sessionData = new();
    private System.Threading.Timer? _telemetryTimer;

    // Properties
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

    // Commands
    public ICommand ConnectCommand { get; }
    public ICommand StartRecordingCommand { get; }
    public ICommand StopRecordingCommand { get; }
    public ICommand AnalyzeSessionCommand { get; }
    public ICommand GenerateSetupCommand { get; }
    public ICommand ExportSetupCommand { get; }

    public MainViewModel(
        ISimulatorConnector acConnector,
        ISimulatorConnector aceConnector,
        ITelemetryAnalyzer telemetryAnalyzer,
        ISetupGenerator setupGenerator)
    {
        _acConnector = acConnector;
        _aceConnector = aceConnector;
        _telemetryAnalyzer = telemetryAnalyzer;
        _setupGenerator = setupGenerator;

        // Initialize collections
        AvailableGames = new ObservableCollection<GameType>
        {
            GameType.AssettoCorsaOriginal,
            GameType.AssettoCorساEvo
        };
        AvailableCars = new ObservableCollection<string>();
        AvailableTracks = new ObservableCollection<string>();

        // Initialize commands
        ConnectCommand = new RelayCommand(_ => ConnectToSimulator(), _ => !IsConnected);
        StartRecordingCommand = new RelayCommand(_ => StartRecording(), _ => IsConnected && !IsRecording);
        StopRecordingCommand = new RelayCommand(_ => StopRecording(), _ => IsRecording);
        AnalyzeSessionCommand = new RelayCommand(_ => AnalyzeSession(), _ => _sessionData.Count > 0);
        GenerateSetupCommand = new RelayCommand(_ => GenerateSetup(), _ => LastAnalysis != null);
        ExportSetupCommand = new RelayCommand(_ => ExportSetup(), _ => LastAnalysis != null);

        // Default selection
        SelectedGame = GameType.AssettoCorsaOriginal;
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
        StatusMessage = $"Recording stopped. {_sessionData.Count} data points captured.";
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

        StatusMessage = "Generating optimized setup...";

        try
        {
            var setup = _setupGenerator.GenerateSetup(LastAnalysis, SelectedCar, SelectedTrack, SelectedGame);
            StatusMessage = "Setup generated successfully!";

            MessageBox.Show(
                $"Setup Generated!\n\n" +
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

    private async void ExportSetup()
    {
        if (LastAnalysis == null || string.IsNullOrEmpty(SelectedCar) || string.IsNullOrEmpty(SelectedTrack))
        {
            MessageBox.Show("Please generate a setup first.", "No Setup", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var setup = _setupGenerator.GenerateSetup(LastAnalysis, SelectedCar, SelectedTrack, SelectedGame);
            
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = $"{SelectedCar}_{SelectedTrack}_setup",
                DefaultExt = ".ini",
                Filter = "Setup files (*.ini)|*.ini|All files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                var success = await _setupGenerator.ExportSetupAsync(setup, dialog.FileName);
                
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

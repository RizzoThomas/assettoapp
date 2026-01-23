using AssettoApp.Core.Models;

namespace AssettoApp.Core.Interfaces;

/// <summary>
/// Interface for reading telemetry data from simulators
/// </summary>
public interface ISimulatorConnector
{
    /// <summary>
    /// Gets the type of game this connector supports
    /// </summary>
    GameType SupportedGame { get; }
    
    /// <summary>
    /// Check if the simulator is currently running
    /// </summary>
    bool IsSimulatorRunning();
    
    /// <summary>
    /// Connect to the simulator's shared memory or telemetry API
    /// </summary>
    Task<bool> ConnectAsync();
    
    /// <summary>
    /// Disconnect from the simulator
    /// </summary>
    void Disconnect();
    
    /// <summary>
    /// Read current telemetry data
    /// </summary>
    TelemetryData? ReadTelemetry();
    
    /// <summary>
    /// Get available cars for the current game
    /// </summary>
    Task<List<string>> GetAvailableCarsAsync();
    
    /// <summary>
    /// Get available tracks for the current game
    /// </summary>
    Task<List<string>> GetAvailableTracksAsync();
}

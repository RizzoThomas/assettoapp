using AssettoApp.Core.Models;

namespace AssettoApp.Core.Interfaces;

/// <summary>
/// Service for tracking laps and building session history from telemetry
/// </summary>
public interface ILapTracker
{
    /// <summary>
    /// Current session being tracked
    /// </summary>
    SessionHistory? CurrentSession { get; }
    
    /// <summary>
    /// Start tracking a new session
    /// </summary>
    void StartSession(GameType gameType, string carName, string trackName);
    
    /// <summary>
    /// Process telemetry data point and update lap tracking
    /// </summary>
    void ProcessTelemetry(TelemetryData telemetry);
    
    /// <summary>
    /// End current session and return the completed session
    /// </summary>
    SessionHistory? EndSession();
    
    /// <summary>
    /// Get current lap in progress
    /// </summary>
    LapData? GetCurrentLap();
    
    /// <summary>
    /// Check if a lap is considered perfect based on criteria
    /// </summary>
    bool IsPerfectLap(LapData lap);
}

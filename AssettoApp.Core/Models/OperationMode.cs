namespace AssettoApp.Core.Models;

/// <summary>
/// Operating mode for the application
/// </summary>
public enum OperationMode
{
    /// <summary>
    /// In-game mode: Simulator is running, reads real telemetry data
    /// </summary>
    InGame,
    
    /// <summary>
    /// Offline mode: Simulator not running, uses preset data and manual selection
    /// </summary>
    Offline
}

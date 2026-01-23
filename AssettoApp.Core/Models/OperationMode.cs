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
    /// Online Analysis mode: Simulator not running, uses online data sources + local presets
    /// Internet connection available for fetching setup databases and community data
    /// </summary>
    OnlineAnalysis
}

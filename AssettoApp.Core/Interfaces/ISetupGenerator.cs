using AssettoApp.Core.Models;

namespace AssettoApp.Core.Interfaces;

/// <summary>
/// Interface for generating optimized car setups
/// </summary>
public interface ISetupGenerator
{
    /// <summary>
    /// Generate an optimized setup based on telemetry analysis (In-Game mode)
    /// </summary>
    CarSetup GenerateSetup(
        TelemetryAnalysisResult analysisResult,
        string carName,
        string trackName,
        GameType gameType);
    
    /// <summary>
    /// Generate a preset-based setup without telemetry data (Offline mode)
    /// Uses car and track characteristics from knowledge base
    /// </summary>
    CarSetup GenerateOfflineSetup(
        string carName,
        string trackName,
        GameType gameType,
        string drivingStylePreference = "Balanced");
    
    /// <summary>
    /// Export setup to a file format compatible with the simulator
    /// </summary>
    Task<bool> ExportSetupAsync(CarSetup setup, string outputPath);
}

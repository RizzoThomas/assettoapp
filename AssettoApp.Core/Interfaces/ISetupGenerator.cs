using AssettoApp.Core.Models;

namespace AssettoApp.Core.Interfaces;

/// <summary>
/// Interface for generating optimized car setups
/// </summary>
public interface ISetupGenerator
{
    /// <summary>
    /// Generate an optimized setup based on telemetry analysis
    /// </summary>
    CarSetup GenerateSetup(
        TelemetryAnalysisResult analysisResult,
        string carName,
        string trackName,
        GameType gameType);
    
    /// <summary>
    /// Export setup to a file format compatible with the simulator
    /// </summary>
    Task<bool> ExportSetupAsync(CarSetup setup, string outputPath);
}

using AssettoApp.Core.Interfaces;
using AssettoApp.Core.Models;
using System.Diagnostics;

namespace AssettoApp.SimulatorIntegration.Connectors;

/// <summary>
/// Connector for Assetto Corsa EVO
/// NOTE: ACE is still in early access and telemetry API may be limited or evolving
/// This implementation provides a framework with fallback strategies
/// </summary>
public class AssettoCorساEvoConnector : ISimulatorConnector
{
    public GameType SupportedGame => GameType.AssettoCorساEvo;

    public bool IsSimulatorRunning()
    {
        // Check if ACE process is running
        // Process name is AssettoCorsaEVO.exe
        var processes = Process.GetProcessesByName("AssettoCorsaEVO");
        return processes.Length > 0;
    }

    public async Task<bool> ConnectAsync()
    {
        await Task.Delay(100);
        
        // TECHNICAL LIMITATION: As of early 2024, Assetto Corsa EVO telemetry API
        // is not yet fully documented or available in early access
        // 
        // Possible approaches:
        // 1. Wait for official shared memory API (similar to AC1)
        // 2. Use UDP telemetry if available
        // 3. Use game's internal replay/telemetry files if accessible
        // 4. Monitor for community-developed tools
        
        // For now, return false to indicate unavailable
        // This should be updated when ACE telemetry becomes available
        return false;
    }

    public void Disconnect()
    {
        // Placeholder for future implementation
    }

    public TelemetryData? ReadTelemetry()
    {
        // TECHNICAL LIMITATION: ACE telemetry not yet available
        // 
        // When ACE telemetry API is released, this method should:
        // 1. Read from shared memory (if similar to AC1)
        // 2. Parse UDP packets (if UDP-based)
        // 3. Read from replay files (if file-based)
        
        // For now, return null indicating unavailable data
        return null;
    }

    public async Task<List<string>> GetAvailableCarsAsync()
    {
        await Task.CompletedTask;
        
        // ACE cars - this list should be updated as ACE content is released
        // Currently returning expected cars based on early access information
        return new List<string>
        {
            "alfa_romeo_giulia_gta",
            "ferrari_296_gtb",
            "porsche_992_gt3",
            "bmw_m4_csl",
            "mercedes_amg_gt_black_series"
        };
    }

    public async Task<List<string>> GetAvailableTracksAsync()
    {
        await Task.CompletedTask;
        
        // ACE tracks - update as content is released
        return new List<string>
        {
            "nurburgring_nordschleife",
            "brands_hatch",
            "spa_francorchamps",
            "imola"
        };
    }
}

using System.Diagnostics;
using AssettoApp.Core.Models;

namespace AssettoApp.Core.Utilities;

/// <summary>
/// Detects running simulator processes
/// </summary>
public static class ProcessDetector
{
    private const string ACProcessName = "acs";
    private const string ACEProcessName = "AssettoCorsaEVO";

    /// <summary>
    /// Check if any supported simulator is currently running
    /// </summary>
    /// <returns>Tuple of (isRunning, detectedGame)</returns>
    public static (bool IsRunning, GameType? DetectedGame) DetectRunningSimulator()
    {
        try
        {
            // Check for Assetto Corsa
            var acProcesses = Process.GetProcessesByName(ACProcessName);
            if (acProcesses.Length > 0)
            {
                return (true, GameType.AssettoCorsaOriginal);
            }

            // Check for Assetto Corsa EVO
            var aceProcesses = Process.GetProcessesByName(ACEProcessName);
            if (aceProcesses.Length > 0)
            {
                return (true, GameType.AssettoCorساEvo);
            }

            return (false, null);
        }
        catch
        {
            // If we can't detect processes, assume nothing is running
            return (false, null);
        }
    }

    /// <summary>
    /// Check if a specific simulator is running
    /// </summary>
    public static bool IsSimulatorRunning(GameType gameType)
    {
        try
        {
            var processName = gameType == GameType.AssettoCorsaOriginal ? ACProcessName : ACEProcessName;
            var processes = Process.GetProcessesByName(processName);
            return processes.Length > 0;
        }
        catch
        {
            return false;
        }
    }
}

using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text;

namespace AssettoApp.ACEInjector;

/// <summary>
/// Main entry point for the injectable DLL
/// This DLL gets injected into the Assetto Corsa Evo process
/// </summary>
public static class InjectorMain
{
    private static Thread? _telemetryThread;
    private static bool _running = false;
    private static IntPtr _gameBaseAddress = IntPtr.Zero;

    /// <summary>
    /// DLL entry point called when DLL is loaded into the process
    /// </summary>
    [DllExport("DllMain", CallingConvention = CallingConvention.StdCall)]
    public static bool DllMain(IntPtr hinstDLL, uint fdwReason, IntPtr lpvReserved)
    {
        const uint DLL_PROCESS_ATTACH = 1;
        const uint DLL_PROCESS_DETACH = 0;

        switch (fdwReason)
        {
            case DLL_PROCESS_ATTACH:
                Initialize();
                break;

            case DLL_PROCESS_DETACH:
                Shutdown();
                break;
        }

        return true;
    }

    /// <summary>
    /// Initialize the injector
    /// </summary>
    private static void Initialize()
    {
        try
        {
            // Get the base address of the main executable
            var currentProcess = Process.GetCurrentProcess();
            _gameBaseAddress = currentProcess.MainModule?.BaseAddress ?? IntPtr.Zero;

            if (_gameBaseAddress == IntPtr.Zero)
            {
                LogError("Failed to get game base address");
                return;
            }

            // Start telemetry reading thread
            _running = true;
            _telemetryThread = new Thread(TelemetryThreadLoop)
            {
                IsBackground = true,
                Name = "ACE Telemetry Reader"
            };
            _telemetryThread.Start();

            LogInfo($"ACE Injector initialized successfully (Base: 0x{_gameBaseAddress:X})");
        }
        catch (Exception ex)
        {
            LogError($"Failed to initialize: {ex.Message}");
        }
    }

    /// <summary>
    /// Shutdown and cleanup
    /// </summary>
    private static void Shutdown()
    {
        _running = false;
        _telemetryThread?.Join(1000);
        LogInfo("ACE Injector shut down");
    }

    /// <summary>
    /// Main telemetry reading loop
    /// </summary>
    private static void TelemetryThreadLoop()
    {
        while (_running)
        {
            try
            {
                // Read telemetry data from game memory
                // This will be implemented with pattern scanning
                ReadAndPublishTelemetry();

                // Read at 100Hz (10ms interval)
                Thread.Sleep(10);
            }
            catch (Exception ex)
            {
                LogError($"Telemetry thread error: {ex.Message}");
                Thread.Sleep(1000); // Back off on error
            }
        }
    }

    /// <summary>
    /// Read telemetry from game memory and publish via IPC
    /// </summary>
    private static void ReadAndPublishTelemetry()
    {
        // TODO: Implement pattern scanning and memory reading
        // For now, this is a placeholder
        
        // Example of what we'd read:
        // - Car position/rotation
        // - Speed, RPM, gear
        // - Tire temperatures and pressures
        // - Suspension travel
        // - Lap times
        // etc.
    }

    /// <summary>
    /// Export function to get telemetry data
    /// Can be called from the main app via function pointer
    /// </summary>
    [DllExport("GetTelemetryData", CallingConvention = CallingConvention.Cdecl)]
    public static IntPtr GetTelemetryData()
    {
        // Return pointer to shared telemetry structure
        // Main app can read this
        return IntPtr.Zero; // TODO: Implement
    }

    /// <summary>
    /// Export function to check if injector is alive
    /// </summary>
    [DllExport("IsInjectorActive", CallingConvention = CallingConvention.Cdecl)]
    public static bool IsInjectorActive()
    {
        return _running;
    }

    private static void LogInfo(string message)
    {
        File.AppendAllText("ace_injector.log", $"[INFO] {DateTime.Now:HH:mm:ss} {message}\n");
    }

    private static void LogError(string message)
    {
        File.AppendAllText("ace_injector.log", $"[ERROR] {DateTime.Now:HH:mm:ss} {message}\n");
    }
}

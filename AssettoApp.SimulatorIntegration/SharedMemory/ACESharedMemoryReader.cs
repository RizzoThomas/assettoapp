using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace AssettoApp.SimulatorIntegration.SharedMemory;

/// <summary>
/// Reader for Assetto Corsa EVO shared memory
/// ACE uses similar memory-mapped file approach as AC but with potentially different names
/// </summary>
public class ACESharedMemoryReader : IDisposable
{
    private MemoryMappedFile? _physicsMMF;
    private MemoryMappedFile? _graphicsMMF;
    private MemoryMappedFile? _staticMMF;
    private bool _isConnected;

    // ACE may use different names - trying common patterns
    // Pattern 1: Similar to AC with "ace" prefix
    private const string PhysicsMMFName = "Local\\acpmf_physics";
    private const string GraphicsMMFName = "Local\\acpmf_graphics";
    private const string StaticMMFName = "Local\\acpmf_static";
    
    // Alternative patterns for ACE if above don't work
    private const string AltPhysicsMMFName = "Local\\acepmf_physics";
    private const string AltGraphicsMMFName = "Local\\acepmf_graphics";
    private const string AltStaticMMFName = "Local\\acepmf_static";
    
    // Another possible pattern
    private const string Alt2PhysicsMMFName = "Local\\ac2pmf_physics";
    private const string Alt2GraphicsMMFName = "Local\\ac2pmf_graphics";
    private const string Alt2StaticMMFName = "Local\\ac2pmf_static";

    public bool IsConnected => _isConnected;

    public bool Connect()
    {
        try
        {
            // Platform check - this code only runs on Windows
            if (!OperatingSystem.IsWindows())
            {
                throw new PlatformNotSupportedException("Assetto Corsa EVO shared memory is only available on Windows.");
            }

            // Try primary pattern (same as AC - ACE might use same API)
            if (TryConnectWithPattern(PhysicsMMFName, GraphicsMMFName, StaticMMFName))
            {
                _isConnected = true;
                return true;
            }

            // Try alternative pattern with "ace" prefix
            if (TryConnectWithPattern(AltPhysicsMMFName, AltGraphicsMMFName, AltStaticMMFName))
            {
                _isConnected = true;
                return true;
            }

            // Try alternative pattern with "ac2" prefix
            if (TryConnectWithPattern(Alt2PhysicsMMFName, Alt2GraphicsMMFName, Alt2StaticMMFName))
            {
                _isConnected = true;
                return true;
            }

            _isConnected = false;
            return false;
        }
        catch (FileNotFoundException)
        {
            _isConnected = false;
            return false;
        }
    }

    private bool TryConnectWithPattern(string physicsName, string graphicsName, string staticName)
    {
        try
        {
            _physicsMMF = MemoryMappedFile.OpenExisting(physicsName, MemoryMappedFileRights.Read);
            _graphicsMMF = MemoryMappedFile.OpenExisting(graphicsName, MemoryMappedFileRights.Read);
            _staticMMF = MemoryMappedFile.OpenExisting(staticName, MemoryMappedFileRights.Read);
            return true;
        }
        catch
        {
            // Clean up any partial connections
            _physicsMMF?.Dispose();
            _graphicsMMF?.Dispose();
            _staticMMF?.Dispose();
            _physicsMMF = null;
            _graphicsMMF = null;
            _staticMMF = null;
            return false;
        }
    }

    public ACPhysics? ReadPhysics()
    {
        if (!_isConnected || _physicsMMF == null) return null;

        try
        {
            using var accessor = _physicsMMF.CreateViewAccessor(0, Marshal.SizeOf<ACPhysics>(), MemoryMappedFileAccess.Read);
            accessor.Read(0, out ACPhysics physics);
            return physics;
        }
        catch
        {
            return null;
        }
    }

    public ACGraphics? ReadGraphics()
    {
        if (!_isConnected || _graphicsMMF == null) return null;

        try
        {
            using var accessor = _graphicsMMF.CreateViewAccessor(0, Marshal.SizeOf<ACGraphics>(), MemoryMappedFileAccess.Read);
            accessor.Read(0, out ACGraphics graphics);
            return graphics;
        }
        catch
        {
            return null;
        }
    }

    public ACStatic? ReadStatic()
    {
        if (!_isConnected || _staticMMF == null) return null;

        try
        {
            using var accessor = _staticMMF.CreateViewAccessor(0, Marshal.SizeOf<ACStatic>(), MemoryMappedFileAccess.Read);
            accessor.Read(0, out ACStatic staticInfo);
            return staticInfo;
        }
        catch
        {
            return null;
        }
    }

    public void Disconnect()
    {
        _physicsMMF?.Dispose();
        _graphicsMMF?.Dispose();
        _staticMMF?.Dispose();
        _physicsMMF = null;
        _graphicsMMF = null;
        _staticMMF = null;
        _isConnected = false;
    }

    public void Dispose()
    {
        Disconnect();
        GC.SuppressFinalize(this);
    }
}

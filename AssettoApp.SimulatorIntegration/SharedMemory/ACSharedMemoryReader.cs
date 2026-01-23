using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace AssettoApp.SimulatorIntegration.SharedMemory;

/// <summary>
/// Reader for Assetto Corsa shared memory
/// Supports reading telemetry data from AC using memory-mapped files
/// </summary>
public class ACSharedMemoryReader : IDisposable
{
    private MemoryMappedFile? _physicsMMF;
    private MemoryMappedFile? _graphicsMMF;
    private MemoryMappedFile? _staticMMF;
    private bool _isConnected;

    private const string PhysicsMMFName = "Local\\acpmf_physics";
    private const string GraphicsMMFName = "Local\\acpmf_graphics";
    private const string StaticMMFName = "Local\\acpmf_static";

    public bool IsConnected => _isConnected;

    public bool Connect()
    {
        try
        {
            _physicsMMF = MemoryMappedFile.OpenExisting(PhysicsMMFName, MemoryMappedFileRights.Read);
            _graphicsMMF = MemoryMappedFile.OpenExisting(GraphicsMMFName, MemoryMappedFileRights.Read);
            _staticMMF = MemoryMappedFile.OpenExisting(StaticMMFName, MemoryMappedFileRights.Read);
            _isConnected = true;
            return true;
        }
        catch (FileNotFoundException)
        {
            _isConnected = false;
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

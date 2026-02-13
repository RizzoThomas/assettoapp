using System.Runtime.InteropServices;
using System.Diagnostics;

namespace AssettoApp.ACEInjector.Memory;

/// <summary>
/// Memory pattern scanner for finding data structures in ACE memory
/// Patterns need to be updated for each ACE version
/// </summary>
public unsafe class MemoryScanner
{
    private readonly IntPtr _baseAddress;
    private readonly long _moduleSize;

    public MemoryScanner(IntPtr baseAddress, long moduleSize)
    {
        _baseAddress = baseAddress;
        _moduleSize = moduleSize;
    }

    /// <summary>
    /// Scan memory for a byte pattern
    /// Returns the address of the first match, or IntPtr.Zero if not found
    /// </summary>
    public IntPtr ScanPattern(byte[] pattern, string mask)
    {
        if (pattern.Length != mask.Length)
            throw new ArgumentException("Pattern and mask must be the same length");

        byte* basePtr = (byte*)_baseAddress;
        long scanSize = _moduleSize - pattern.Length;

        for (long i = 0; i < scanSize; i++)
        {
            bool found = true;
            for (int j = 0; j < pattern.Length; j++)
            {
                if (mask[j] == 'x' && basePtr[i + j] != pattern[j])
                {
                    found = false;
                    break;
                }
            }

            if (found)
                return (IntPtr)(basePtr + i);
        }

        return IntPtr.Zero;
    }

    /// <summary>
    /// Read a value from memory
    /// </summary>
    public T Read<T>(IntPtr address) where T : unmanaged
    {
        return *(T*)address;
    }

    /// <summary>
    /// Follow a pointer chain
    /// </summary>
    public IntPtr FollowPointerPath(IntPtr baseAddr, params int[] offsets)
    {
        IntPtr current = baseAddr;

        foreach (var offset in offsets)
        {
            if (current == IntPtr.Zero)
                return IntPtr.Zero;

            current = (IntPtr)(*(long*)current + offset);
        }

        return current;
    }
}

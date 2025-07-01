using System;
using System.Runtime.InteropServices;

namespace Modding;

[StructLayout(LayoutKind.Sequential)]
internal struct RepackStats {
    public int objectsBefore;
    public int objectsAfter;
}

internal class UnitySceneRepackerException(string message) : Exception(message);

internal static class UnitySceneRepacker {

    public enum Mode {
        SceneBundle,
        AssetBundle,
    }

    
    public static (byte[], RepackStats) Repack(string bundleName, string gamePath, string preloadsJson, Mode mode) {
        export(
            bundleName,
            gamePath,
            preloadsJson,
            out IntPtr errorPtr,
            out int bundleSize,
            out IntPtr bundleData,
            out RepackStats stats,
            (byte) mode
        );

        if (errorPtr != IntPtr.Zero) {
            string error = PtrToStringAndFree(errorPtr)!;
            throw new UnitySceneRepackerException(error);
        } else {
            byte[] bytes = PtrToByteArrayAndFree(bundleSize, bundleData);
            return (bytes, stats);
        }
    }


    [DllImport("unityscenerepacker", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void export(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string bundleName,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string gameDir,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string preloadJson,
        out IntPtr error,
        out int bundleSize,
        out IntPtr bundleData,
        out RepackStats repackStats,
        byte mode
        );

    [DllImport("unityscenerepacker", CallingConvention = CallingConvention.Cdecl)]
    private static extern void free_str(IntPtr str);

    [DllImport("unityscenerepacker", CallingConvention = CallingConvention.Cdecl)]
    private static extern void free_array(int len, IntPtr data);

    private static string PtrToStringAndFree(IntPtr ptr) {
        if (ptr == IntPtr.Zero) return null;

        string message = Marshal.PtrToStringAnsi(ptr);
        free_str(ptr);
        return message;
    }

    private static byte[] PtrToByteArrayAndFree(int size, IntPtr ptr) {
        if (ptr == IntPtr.Zero || size == 0)
            return [];

        byte[] managedArray = new byte[size];
        Marshal.Copy(ptr, managedArray, 0, size);
        free_array(size, ptr);
        return managedArray;
    }
}

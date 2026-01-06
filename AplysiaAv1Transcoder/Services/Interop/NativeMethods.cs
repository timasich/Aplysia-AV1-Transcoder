using System.Runtime.InteropServices;

namespace AplysiaAv1Transcoder.Services.Interop;

internal static class NativeMethods
{
    internal const uint STGM_READ = 0x00000000;
    internal const uint GPS_DEFAULT = 0x00000000;
    internal const int S_OK = 0;
    internal const int S_FALSE = 1;
    internal const int RPC_E_CHANGED_MODE = unchecked((int)0x80010106);

    internal enum COINIT : uint
    {
        MULTITHREADED = 0x0,
        APARTMENTTHREADED = 0x2
    }

    [DllImport("ole32.dll")]
    internal static extern int CoInitializeEx(IntPtr pvReserved, COINIT dwCoInit);

    [DllImport("ole32.dll")]
    internal static extern void CoUninitialize();

    [DllImport("ole32.dll")]
    internal static extern int PropVariantClear(ref PropVariant pvar);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    internal static extern int SHGetPropertyStoreFromParsingName(
        string pszPath,
        IntPtr pbc,
        uint flags,
        ref Guid riid,
        out IPropertyStore propertyStore);

    [DllImport("mfplat.dll", PreserveSig = false)]
    internal static extern void MFStartup(int version, int flags);

    [DllImport("mfplat.dll", PreserveSig = false)]
    internal static extern void MFShutdown();

    [DllImport("mfreadwrite.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
    internal static extern void MFCreateSourceReaderFromURL(
        string pwszURL,
        IntPtr pAttributes,
        out IMFSourceReader ppSourceReader);
}

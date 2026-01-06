using System.Runtime.InteropServices;

namespace AplysiaAv1Transcoder.Services.Interop;

[ComImport]
[Guid("70AE66F2-C809-4E4F-8915-BDCB406B7993")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IMFSourceReader
{
    void GetStreamSelection(int dwStreamIndex, out bool pfSelected);
    void SetStreamSelection(int dwStreamIndex, bool fSelected);
    void GetNativeMediaType(int dwStreamIndex, int dwMediaTypeIndex, out IntPtr ppMediaType);
    void GetCurrentMediaType(int dwStreamIndex, out IntPtr ppMediaType);
    void SetCurrentMediaType(int dwStreamIndex, IntPtr pdwReserved, IntPtr pMediaType);
    void SetCurrentPosition(ref Guid guidTimeFormat, ref PropVariant varPosition);
    void ReadSample(int dwStreamIndex, int dwControlFlags, out int pdwActualStreamIndex, out int pdwStreamFlags, out long pllTimestamp, out IntPtr ppSample);
    void Flush(int dwStreamIndex);
    void GetServiceForStream(int dwStreamIndex, ref Guid guidService, ref Guid riid, out IntPtr ppvObject);
    void GetPresentationAttribute(int dwStreamIndex, ref Guid guidAttribute, out PropVariant pvAttribute);
}

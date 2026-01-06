using System.Runtime.InteropServices;

namespace AplysiaAv1Transcoder.Services.Interop;

[StructLayout(LayoutKind.Explicit)]
public struct PropVariant
{
    [FieldOffset(0)]
    public ushort vt;

    [FieldOffset(8)]
    public long hVal;

    [FieldOffset(8)]
    public ulong uhVal;

    public long GetLong()
    {
        return hVal;
    }

    public ulong GetULong()
    {
        return uhVal;
    }
}

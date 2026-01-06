namespace AplysiaAv1Transcoder.Models;

public enum TargetCodec
{
    H264,
    H265
}

public enum EncoderPriority
{
    AutoHW,
    NVENC,
    QSV,
    AMF,
    CPU
}

public enum AudioMode
{
    Copy,
    Aac192
}

public enum BitrateMode
{
    FixedKbps,
    MultiplierFromSource
}

public sealed class Preset
{
    public Guid Id { get; set; } = Guid.Empty;
    public string Name { get; set; } = string.Empty;
    public TargetCodec TargetCodec { get; set; } = TargetCodec.H264;
    public BitrateMode BitrateMode { get; set; } = BitrateMode.FixedKbps;
    public int BitrateKbps { get; set; } = 8000;
    public double Multiplier { get; set; } = 1.0;
    public string PixelFormat { get; set; } = "yuv420p";
    public AudioMode AudioMode { get; set; } = AudioMode.Copy;
    public bool ForceDav1d { get; set; } = true;

    public Preset Clone(string? newName = null)
    {
        return new Preset
        {
            Id = Id,
            Name = newName ?? Name,
            TargetCodec = TargetCodec,
            BitrateMode = BitrateMode,
            BitrateKbps = BitrateKbps,
            Multiplier = Multiplier,
            PixelFormat = PixelFormat,
            AudioMode = AudioMode,
            ForceDav1d = ForceDav1d
        };
    }
}

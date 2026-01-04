using System.Text.Json.Serialization;

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

public sealed class Preset
{
    public string Name { get; set; } = string.Empty;
    public TargetCodec TargetCodec { get; set; } = TargetCodec.H264;
    public EncoderPriority EncoderPriority { get; set; } = EncoderPriority.AutoHW;
    public int BitrateKbps { get; set; } = 8000;
    public string NvencPreset { get; set; } = "p5";
    public string PixelFormat { get; set; } = "yuv420p";
    public AudioMode AudioMode { get; set; } = AudioMode.Copy;
    public bool ForceDav1d { get; set; } = true;

    [JsonIgnore]
    public bool IsBuiltInAuto { get; set; }

    public Preset Clone(string? newName = null)
    {
        return new Preset
        {
            Name = newName ?? Name,
            TargetCodec = TargetCodec,
            EncoderPriority = EncoderPriority,
            BitrateKbps = BitrateKbps,
            NvencPreset = NvencPreset,
            PixelFormat = PixelFormat,
            AudioMode = AudioMode,
            ForceDav1d = ForceDav1d,
            IsBuiltInAuto = IsBuiltInAuto
        };
    }
}

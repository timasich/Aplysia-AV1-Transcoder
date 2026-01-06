using System.Text.Json.Serialization;

namespace AplysiaAv1Transcoder.Models;

public sealed class AppSettings
{
    public string? FfmpegPath { get; set; }
    public string? LastOutputFolder { get; set; }
    public List<string> RecentOutputFolders { get; set; } = new();
    public bool AutoMatchForNewFiles { get; set; } = true;
    public AutoMatchMode AutoMatchMode { get; set; } = AutoMatchMode.Balanced;
    public int AutoMatchBias { get; set; } = 0;
    public TargetCodec DefaultTargetCodec { get; set; } = TargetCodec.H264;
    public string? LastSelectedPreset { get; set; }
    public int? QueueSplitterDistance { get; set; }

    [JsonIgnore]
    public string? ResolvedFfmpegPath { get; set; }

    [JsonIgnore]
    public string? ResolvedFfprobePath { get; set; }
}

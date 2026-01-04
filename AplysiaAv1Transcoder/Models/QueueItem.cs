namespace AplysiaAv1Transcoder.Models;

public sealed class QueueItem
{
    public string FilePath { get; set; } = string.Empty;
    public string PresetName { get; set; } = string.Empty;
    public Preset PresetSnapshot { get; set; } = new();
    public bool TrimEnabled { get; set; }
    public string? TrimStart { get; set; }
    public string? TrimEnd { get; set; }
    public string Status { get; set; } = "Queued";
    public ProbeInfo? ProbeInfo { get; set; }
    public int? AutoMatchedBitrateKbps { get; set; }
    public string? LastCommandLine { get; set; }
}

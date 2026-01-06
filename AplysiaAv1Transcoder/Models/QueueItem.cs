namespace AplysiaAv1Transcoder.Models;

public sealed class QueueItem
{
    public string FilePath { get; set; } = string.Empty;
    public bool Render { get; set; } = true;
    public Guid PresetId { get; set; }
    public string PresetName { get; set; } = string.Empty;
    public Preset PresetSnapshot { get; set; } = new();
    public string? OutputFolder { get; set; }
    public bool TrimEnabled { get; set; }
    public string? TrimStart { get; set; }
    public string? TrimEnd { get; set; }
    public bool TrimWasEditedByUser { get; set; }
    public TimeSpan? Duration { get; set; }
    public DurationSource DurationSource { get; set; }
    public string Status { get; set; } = "Queued";
    public ProbeInfo? ProbeInfo { get; set; }
    public bool IsAv1 { get; set; } = true;
    public string? LastCommandLine { get; set; }
}

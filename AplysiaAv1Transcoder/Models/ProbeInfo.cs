namespace AplysiaAv1Transcoder.Models;

public sealed class ProbeInfo
{
    public double DurationSeconds { get; set; }
    public double Fps { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int? VideoBitrateKbps { get; set; }
    public int? AudioBitrateKbps { get; set; }
    public int? OverallBitrateKbps { get; set; }
}

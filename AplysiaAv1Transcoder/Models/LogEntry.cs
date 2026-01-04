namespace AplysiaAv1Transcoder.Models;

public enum LogLevel
{
    Info,
    Warning,
    Error,
    Command
}

public sealed class LogEntry
{
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public LogLevel Level { get; set; } = LogLevel.Info;
    public string Message { get; set; } = string.Empty;
}

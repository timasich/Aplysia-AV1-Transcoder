using AplysiaAv1Transcoder.Models;

namespace AplysiaAv1Transcoder.Services;

public sealed class DurationService
{
    private readonly IReadOnlyList<(DurationSource source, IVideoDurationProvider provider)> _providers;
    private readonly Action<LogEntry>? _log;

    public DurationService(IEnumerable<(DurationSource source, IVideoDurationProvider provider)> providers, Action<LogEntry>? log = null)
    {
        _providers = providers.ToList();
        _log = log;
    }

    public async Task<(TimeSpan? duration, DurationSource source)> TryGetDurationAsync(string filePath, CancellationToken ct)
    {
        foreach (var (source, provider) in _providers)
        {
            var duration = await provider.TryGetDurationAsync(filePath, ct);
            if (duration.HasValue && duration.Value > TimeSpan.Zero)
            {
                return (duration, source);
            }
        }

        return (null, DurationSource.Unknown);
    }

    public void LogInfo(string message)
    {
        _log?.Invoke(new LogEntry { Level = LogLevel.Info, Message = message });
    }

    public void LogWarning(string message)
    {
        _log?.Invoke(new LogEntry { Level = LogLevel.Warning, Message = message });
    }
}

using System.Diagnostics;
using System.Globalization;
using System.Text;
using AplysiaAv1Transcoder.Models;

namespace AplysiaAv1Transcoder.Services;

public sealed class FfprobeDurationProvider : IVideoDurationProvider
{
    private readonly Func<string?> _ffprobePathProvider;
    private readonly Action<LogEntry>? _log;

    public FfprobeDurationProvider(Func<string?> ffprobePathProvider, Action<LogEntry>? log = null)
    {
        _ffprobePathProvider = ffprobePathProvider;
        _log = log;
    }

    public async Task<TimeSpan?> TryGetDurationAsync(string filePath, CancellationToken ct)
    {
        var ffprobePath = _ffprobePathProvider();
        if (string.IsNullOrWhiteSpace(ffprobePath) || !File.Exists(ffprobePath))
        {
            _log?.Invoke(new LogEntry { Level = LogLevel.Info, Message = "ffprobe duration unavailable (missing ffprobe.exe)" });
            return null;
        }

        var attempt1 = await RunProbeAsync(ffprobePath, filePath, Array.Empty<string>(), ct);
        if (attempt1.HasValue)
        {
            _log?.Invoke(new LogEntry { Level = LogLevel.Info, Message = "Duration via ffprobe (default scan)" });
            return attempt1;
        }

        var retryArgs = new[] { "-probesize", "50M", "-analyzeduration", "50M" };
        var attempt2 = await RunProbeAsync(ffprobePath, filePath, retryArgs, ct);
        if (attempt2.HasValue)
        {
            _log?.Invoke(new LogEntry { Level = LogLevel.Info, Message = "Duration via ffprobe (extended scan)" });
            return attempt2;
        }

        _log?.Invoke(new LogEntry { Level = LogLevel.Info, Message = "ffprobe duration unavailable" });
        return null;
    }

    private static async Task<TimeSpan?> RunProbeAsync(string ffprobePath, string filePath, string[] extraArgs, CancellationToken ct)
    {
        var psi = new ProcessStartInfo
        {
            FileName = ffprobePath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        psi.ArgumentList.Add("-v");
        psi.ArgumentList.Add("error");
        if (extraArgs.Length > 0)
        {
            foreach (var arg in extraArgs)
            {
                psi.ArgumentList.Add(arg);
            }
        }
        psi.ArgumentList.Add("-show_entries");
        psi.ArgumentList.Add("format=duration");
        psi.ArgumentList.Add("-of");
        psi.ArgumentList.Add("default=noprint_wrappers=1:nokey=1");
        psi.ArgumentList.Add(filePath);

        using var process = new Process { StartInfo = psi };
        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync(ct);
        await process.WaitForExitAsync(ct);

        if (process.ExitCode != 0)
        {
            return null;
        }

        if (!double.TryParse(output.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds))
        {
            return null;
        }

        if (seconds <= 0)
        {
            return null;
        }

        return TimeSpan.FromSeconds(seconds);
    }
}

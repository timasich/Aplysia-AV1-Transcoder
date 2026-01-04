using System.Diagnostics;
using System.Text;
using AplysiaAv1Transcoder.Models;

namespace AplysiaAv1Transcoder.Services;

public sealed class TranscodeResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string CommandLine { get; set; } = string.Empty;
    public List<string> Arguments { get; set; } = new();
}

public sealed class TranscodeService
{
    private bool _libDav1dWarned;

    public TranscodeResult BuildCommand(QueueItem item, string ffmpegPath, EncoderCapabilities capabilities, string outputPath, Action<LogEntry>? log = null)
    {
        var preset = item.PresetSnapshot;
        var bitrate = preset.BitrateKbps;
        var args = new List<string> { "-hide_banner", "-y" };

        if (preset.ForceDav1d && capabilities.HasLibDav1d)
        {
            args.Add("-c:v");
            args.Add("libdav1d");
        }
        else if (preset.ForceDav1d && !_libDav1dWarned)
        {
            _libDav1dWarned = true;
            log?.Invoke(new LogEntry
            {
                Level = LogLevel.Warning,
                Message = "libdav1d not available in this FFmpeg build; using default AV1 decoder."
            });
        }

        args.Add("-i");
        args.Add(item.FilePath);

        if (item.TrimEnabled)
        {
            if (!string.IsNullOrWhiteSpace(item.TrimStart))
            {
                args.Add("-ss");
                args.Add(item.TrimStart!);
            }

            if (!string.IsNullOrWhiteSpace(item.TrimEnd))
            {
                args.Add("-to");
                args.Add(item.TrimEnd!);
            }
        }

        var resolved = ResolveEncoder(preset, capabilities, log);
        args.Add("-c:v");
        args.Add(resolved.encoderName);

        if (bitrate > 0)
        {
            args.Add("-b:v");
            args.Add($"{bitrate}k");
            args.Add("-maxrate");
            args.Add($"{bitrate}k");
            args.Add("-bufsize");
            args.Add($"{bitrate * 2}k");
        }

        if (resolved.usesNvenc)
        {
            args.Add("-preset");
            args.Add(preset.NvencPreset);
        }
        else if (resolved.usesCpu)
        {
            args.Add("-preset");
            args.Add("medium");
        }

        if (!string.IsNullOrWhiteSpace(preset.PixelFormat))
        {
            args.Add("-pix_fmt");
            args.Add(preset.PixelFormat);
        }

        if (preset.AudioMode == AudioMode.Copy)
        {
            args.Add("-c:a");
            args.Add("copy");
        }
        else
        {
            args.Add("-c:a");
            args.Add("aac");
            args.Add("-b:a");
            args.Add("192k");
        }

        args.Add(outputPath);

        var commandLine = BuildCommandLine(ffmpegPath, args);
        return new TranscodeResult { Success = true, CommandLine = commandLine, Arguments = args };
    }

    public async Task<TranscodeResult> RunAsync(QueueItem item, string ffmpegPath, EncoderCapabilities capabilities, string outputPath, Action<LogEntry> log, CancellationToken cancellationToken)
    {
        var build = BuildCommand(item, ffmpegPath, capabilities, outputPath, log);
        build.Success = false;

        item.LastCommandLine = build.CommandLine;
        log(new LogEntry { Level = LogLevel.Command, Message = build.CommandLine });

        var psi = new ProcessStartInfo
        {
            FileName = ffmpegPath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        foreach (var arg in build.Arguments)
        {
            psi.ArgumentList.Add(arg);
        }

        using var process = new Process { StartInfo = psi, EnableRaisingEvents = true };
        var stderrBuffer = new StringBuilder();

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data != null)
            {
                log(new LogEntry { Level = LogLevel.Info, Message = e.Data });
            }
        };

        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data != null)
            {
                stderrBuffer.AppendLine(e.Data);
                var level = e.Data.Contains("error", StringComparison.OrdinalIgnoreCase)
                    ? LogLevel.Error
                    : LogLevel.Info;
                log(new LogEntry { Level = level, Message = e.Data });
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        using var registration = cancellationToken.Register(() =>
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill(true);
                }
            }
            catch
            {
            }
        });

        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode == 0)
        {
            build.Success = true;
            return build;
        }

        build.ErrorMessage = stderrBuffer.ToString();
        return build;
    }

    private static (string encoderName, bool usesNvenc, bool usesCpu) ResolveEncoder(Preset preset, EncoderCapabilities capabilities, Action<LogEntry>? log)
    {
        var codecSuffix = preset.TargetCodec == TargetCodec.H264 ? "h264" : "hevc";

        switch (preset.EncoderPriority)
        {
            case EncoderPriority.NVENC:
                if (IsNvencAvailable(preset.TargetCodec, capabilities))
                {
                    return ($"{codecSuffix}_nvenc", true, false);
                }
                log?.Invoke(new LogEntry { Level = LogLevel.Warning, Message = "NVENC not available. Falling back to CPU." });
                return ResolveCpu(codecSuffix);
            case EncoderPriority.QSV:
                if (IsQsvAvailable(preset.TargetCodec, capabilities))
                {
                    return ($"{codecSuffix}_qsv", false, false);
                }
                log?.Invoke(new LogEntry { Level = LogLevel.Warning, Message = "Intel QSV not available. Falling back to CPU." });
                return ResolveCpu(codecSuffix);
            case EncoderPriority.AMF:
                if (IsAmfAvailable(preset.TargetCodec, capabilities))
                {
                    return ($"{codecSuffix}_amf", false, false);
                }
                log?.Invoke(new LogEntry { Level = LogLevel.Warning, Message = "AMD AMF not available. Falling back to CPU." });
                return ResolveCpu(codecSuffix);
            case EncoderPriority.CPU:
                return ResolveCpu(codecSuffix);
            case EncoderPriority.AutoHW:
            default:
                if (IsNvencAvailable(preset.TargetCodec, capabilities))
                {
                    return ($"{codecSuffix}_nvenc", true, false);
                }
                if (IsQsvAvailable(preset.TargetCodec, capabilities))
                {
                    return ($"{codecSuffix}_qsv", false, false);
                }
                if (IsAmfAvailable(preset.TargetCodec, capabilities))
                {
                    return ($"{codecSuffix}_amf", false, false);
                }
                return ResolveCpu(codecSuffix);
        }
    }

    private static (string encoderName, bool usesNvenc, bool usesCpu) ResolveCpu(string codecSuffix)
    {
        var encoder = codecSuffix == "h264" ? "libx264" : "libx265";
        return (encoder, false, true);
    }

    private static bool IsNvencAvailable(TargetCodec codec, EncoderCapabilities capabilities)
    {
        return codec == TargetCodec.H264 ? capabilities.HasNvencH264 : capabilities.HasNvencH265;
    }

    private static bool IsQsvAvailable(TargetCodec codec, EncoderCapabilities capabilities)
    {
        return codec == TargetCodec.H264 ? capabilities.HasQsvH264 : capabilities.HasQsvH265;
    }

    private static bool IsAmfAvailable(TargetCodec codec, EncoderCapabilities capabilities)
    {
        return codec == TargetCodec.H264 ? capabilities.HasAmfH264 : capabilities.HasAmfH265;
    }

    private static string BuildCommandLine(string exe, IEnumerable<string> args)
    {
        var builder = new StringBuilder();
        builder.Append(Escape(exe));
        foreach (var arg in args)
        {
            builder.Append(' ').Append(Escape(arg));
        }

        return builder.ToString();
    }

    private static string Escape(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "\"\"";
        }

        if (value.Contains(' ') || value.Contains('\\') || value.Contains('"'))
        {
            return $"\"{value.Replace("\"", "\\\"")}\"";
        }

        return value;
    }
}

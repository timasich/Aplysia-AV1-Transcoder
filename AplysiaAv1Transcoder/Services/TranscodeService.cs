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

    public TranscodeResult BuildCommand(QueueItem item, string ffmpegPath, EncoderCapabilities capabilities, string outputPath, EncoderPriority encoderPriority, int speedQuality, Action<LogEntry>? log = null)
    {
        var preset = item.PresetSnapshot;
        var bitrate = ResolveTargetBitrate(preset, item.ProbeInfo, log);
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

        if (item.TrimEnabled && TryParseTrimTime(item.TrimStart, out var trimStart) && TryParseTrimTime(item.TrimEnd, out var trimEnd))
        {
            var applyTrim = trimStart < trimEnd;
            var duration = item.Duration ?? (item.ProbeInfo?.DurationSeconds > 0 ? TimeSpan.FromSeconds(item.ProbeInfo.DurationSeconds) : (TimeSpan?)null);
            if (applyTrim && duration.HasValue && duration.Value > TimeSpan.Zero)
            {
                var fullRange = trimStart == TimeSpan.Zero && Math.Abs((trimEnd - duration.Value).TotalSeconds) < 0.5;
                applyTrim = !fullRange;
            }

            if (applyTrim)
            {
                if (trimStart > TimeSpan.Zero)
                {
                    args.Add("-ss");
                    args.Add(item.TrimStart!);
                }

                if (trimEnd > TimeSpan.Zero)
                {
                    args.Add("-to");
                    args.Add(item.TrimEnd!);
                }
            }
        }

        var resolved = ResolveEncoder(preset.TargetCodec, encoderPriority, capabilities, log);
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

        AppendSpeedQualityArgs(args, resolved.kind, speedQuality);

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

    public async Task<TranscodeResult> RunAsync(QueueItem item, string ffmpegPath, EncoderCapabilities capabilities, string outputPath, EncoderPriority encoderPriority, int speedQuality, Action<LogEntry> log, CancellationToken cancellationToken)
    {
        var build = BuildCommand(item, ffmpegPath, capabilities, outputPath, encoderPriority, speedQuality, log);
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

    private enum EncoderKind
    {
        Nvenc,
        Qsv,
        Amf,
        Cpu
    }

    private static (string encoderName, EncoderKind kind) ResolveEncoder(TargetCodec codec, EncoderPriority encoderPriority, EncoderCapabilities capabilities, Action<LogEntry>? log)
    {
        var codecSuffix = codec == TargetCodec.H264 ? "h264" : "hevc";

        switch (encoderPriority)
        {
            case EncoderPriority.NVENC:
                if (IsNvencAvailable(codec, capabilities))
                {
                    return ($"{codecSuffix}_nvenc", EncoderKind.Nvenc);
                }
                log?.Invoke(new LogEntry { Level = LogLevel.Warning, Message = "NVENC not available. Falling back to CPU." });
                return ResolveCpu(codecSuffix);
            case EncoderPriority.QSV:
                if (IsQsvAvailable(codec, capabilities))
                {
                    return ($"{codecSuffix}_qsv", EncoderKind.Qsv);
                }
                log?.Invoke(new LogEntry { Level = LogLevel.Warning, Message = "Intel QSV not available. Falling back to CPU." });
                return ResolveCpu(codecSuffix);
            case EncoderPriority.AMF:
                if (IsAmfAvailable(codec, capabilities))
                {
                    return ($"{codecSuffix}_amf", EncoderKind.Amf);
                }
                log?.Invoke(new LogEntry { Level = LogLevel.Warning, Message = "AMD AMF not available. Falling back to CPU." });
                return ResolveCpu(codecSuffix);
            case EncoderPriority.CPU:
                return ResolveCpu(codecSuffix);
            case EncoderPriority.AutoHW:
            default:
                if (IsNvencAvailable(codec, capabilities))
                {
                    return ($"{codecSuffix}_nvenc", EncoderKind.Nvenc);
                }
                if (IsQsvAvailable(codec, capabilities))
                {
                    return ($"{codecSuffix}_qsv", EncoderKind.Qsv);
                }
                if (IsAmfAvailable(codec, capabilities))
                {
                    return ($"{codecSuffix}_amf", EncoderKind.Amf);
                }
                return ResolveCpu(codecSuffix);
        }
    }

    public static int ResolveTargetBitrate(Preset preset, ProbeInfo? info, Action<LogEntry>? log = null)
    {
        if (preset.BitrateMode == BitrateMode.FixedKbps)
        {
            return preset.BitrateKbps;
        }

        var sourceKbps = info?.VideoBitrateKbps ?? 0;
        var usedFallback = false;
        if (sourceKbps <= 0)
        {
            sourceKbps = GetFallbackSourceKbps(info);
            usedFallback = true;
        }

        var target = (int)Math.Round(sourceKbps * Math.Clamp(preset.Multiplier, 0.1, 10.0));
        target = Math.Clamp(target, 500, 200000);

        if (usedFallback)
        {
            log?.Invoke(new LogEntry
            {
                Level = LogLevel.Warning,
                Message = $"Source bitrate unknown, using fallback target bitrate {target} kbps."
            });
        }

        return target;
    }

    public static int GetFallbackSourceKbps(ProbeInfo? info)
    {
        if (info == null)
        {
            return 8000;
        }

        var maxDim = Math.Max(info.Width, info.Height);
        if (maxDim >= 2160)
        {
            return 20000;
        }

        if (maxDim >= 1440)
        {
            return 12000;
        }

        if (maxDim >= 1080)
        {
            return 8000;
        }

        if (maxDim >= 720)
        {
            return 4500;
        }

        return 2500;
    }

    private static (string encoderName, EncoderKind kind) ResolveCpu(string codecSuffix)
    {
        var encoder = codecSuffix == "h264" ? "libx264" : "libx265";
        return (encoder, EncoderKind.Cpu);
    }

    private static void AppendSpeedQualityArgs(List<string> args, EncoderKind kind, int speedQuality)
    {
        var value = Math.Clamp(speedQuality, 1, 9);
        switch (kind)
        {
            case EncoderKind.Nvenc:
                args.Add("-preset");
                args.Add($"p{value}");
                break;
            case EncoderKind.Amf:
                args.Add("-quality");
                args.Add(ResolveAmfQuality(value));
                break;
            case EncoderKind.Qsv:
                args.Add("-preset");
                args.Add(ResolveQsvPreset(value));
                break;
            case EncoderKind.Cpu:
                args.Add("-preset");
                args.Add(ResolveCpuPreset(value));
                break;
        }
    }

    private static string ResolveAmfQuality(int value)
    {
        if (value <= 3)
        {
            return "speed";
        }

        if (value <= 6)
        {
            return "balanced";
        }

        return "quality";
    }

    private static string ResolveQsvPreset(int value)
    {
        var presets = new[]
        {
            "veryfast",
            "faster",
            "fast",
            "medium",
            "slow",
            "slower",
            "veryslow",
            "veryslow",
            "veryslow"
        };

        return presets[value - 1];
    }

    private static string ResolveCpuPreset(int value)
    {
        var presets = new[]
        {
            "ultrafast",
            "superfast",
            "veryfast",
            "faster",
            "fast",
            "medium",
            "slow",
            "slower",
            "veryslow"
        };

        return presets[value - 1];
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

    private static bool TryParseTrimTime(string? value, out TimeSpan time)
    {
        time = TimeSpan.Zero;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var parts = value.Trim().Split(':');
        if (parts.Length is < 1 or > 3)
        {
            return false;
        }

        var hours = 0;
        var minutes = 0;
        var secondsPart = string.Empty;

        if (parts.Length == 1)
        {
            secondsPart = parts[0];
        }
        else if (parts.Length == 2)
        {
            if (!int.TryParse(parts[0], out minutes))
            {
                return false;
            }
            secondsPart = parts[1];
        }
        else
        {
            if (!int.TryParse(parts[0], out hours) || !int.TryParse(parts[1], out minutes))
            {
                return false;
            }
            secondsPart = parts[2];
        }

        if (hours < 0 || hours > 99 || minutes < 0 || minutes > 59)
        {
            return false;
        }

        var secParts = secondsPart.Split('.');
        if (secParts.Length is < 1 or > 2)
        {
            return false;
        }

        if (!int.TryParse(secParts[0], out var seconds) || seconds < 0 || seconds > 59)
        {
            return false;
        }

        var milliseconds = 0;
        if (secParts.Length == 2 && (!int.TryParse(secParts[1], out milliseconds) || milliseconds < 0 || milliseconds > 999))
        {
            return false;
        }

        time = new TimeSpan(0, hours, minutes, seconds, milliseconds);
        return true;
    }
}

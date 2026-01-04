using System.Diagnostics;
using System.Text;
using System.Text.Json;
using AplysiaAv1Transcoder.Models;

namespace AplysiaAv1Transcoder.Services;

public sealed class FfprobeService
{
    public async Task<ProbeInfo?> ProbeAsync(string ffprobePath, string filePath, CancellationToken cancellationToken)
    {
        if (!File.Exists(filePath))
        {
            return null;
        }

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

        using var process = new Process { StartInfo = psi };
        psi.ArgumentList.Add("-v");
        psi.ArgumentList.Add("error");
        psi.ArgumentList.Add("-print_format");
        psi.ArgumentList.Add("json");
        psi.ArgumentList.Add("-show_entries");
        psi.ArgumentList.Add("format=duration,bit_rate");
        psi.ArgumentList.Add("-show_streams");
        psi.ArgumentList.Add(filePath);
        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        var error = await process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            return null;
        }

        return ParseProbe(output);
    }

    private static ProbeInfo? ParseProbe(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var info = new ProbeInfo();

        if (root.TryGetProperty("format", out var format))
        {
            if (format.TryGetProperty("duration", out var durationElement) &&
                double.TryParse(durationElement.GetString(), out var duration))
            {
                info.DurationSeconds = duration;
            }

            if (format.TryGetProperty("bit_rate", out var overallBitrateElement) &&
                int.TryParse(overallBitrateElement.GetString(), out var overallBitrate))
            {
                info.OverallBitrateKbps = overallBitrate / 1000;
            }
        }

        if (root.TryGetProperty("streams", out var streams) && streams.ValueKind == JsonValueKind.Array)
        {
            foreach (var stream in streams.EnumerateArray())
            {
                if (!stream.TryGetProperty("codec_type", out var typeElement))
                {
                    continue;
                }

                var codecType = typeElement.GetString();
                if (string.Equals(codecType, "video", StringComparison.OrdinalIgnoreCase))
                {
                    if (stream.TryGetProperty("width", out var widthElement))
                    {
                        info.Width = widthElement.GetInt32();
                    }

                    if (stream.TryGetProperty("height", out var heightElement))
                    {
                        info.Height = heightElement.GetInt32();
                    }

                    if (stream.TryGetProperty("bit_rate", out var bitrateElement) &&
                        int.TryParse(bitrateElement.GetString(), out var videoBitrate))
                    {
                        info.VideoBitrateKbps = videoBitrate / 1000;
                    }

                    if (stream.TryGetProperty("avg_frame_rate", out var fpsElement))
                    {
                        info.Fps = ParseFps(fpsElement.GetString());
                    }
                    else if (stream.TryGetProperty("r_frame_rate", out var rFpsElement))
                    {
                        info.Fps = ParseFps(rFpsElement.GetString());
                    }
                }
                else if (string.Equals(codecType, "audio", StringComparison.OrdinalIgnoreCase))
                {
                    if (stream.TryGetProperty("bit_rate", out var audioBitrateElement) &&
                        int.TryParse(audioBitrateElement.GetString(), out var audioBitrate))
                    {
                        info.AudioBitrateKbps = audioBitrate / 1000;
                    }
                }
            }
        }

        return info;
    }

    private static double ParseFps(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0;
        }

        var parts = value.Split('/');
        if (parts.Length == 2 &&
            double.TryParse(parts[0], out var numerator) &&
            double.TryParse(parts[1], out var denominator) &&
            denominator > 0)
        {
            return numerator / denominator;
        }

        if (double.TryParse(value, out var fps))
        {
            return fps;
        }

        return 0;
    }
}

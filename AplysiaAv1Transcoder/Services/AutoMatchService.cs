using System.Diagnostics;
using AplysiaAv1Transcoder.Models;

namespace AplysiaAv1Transcoder.Services;

public readonly record struct AutoMatchResult(int TargetKbps, int SourceKbps, double Scale);

public static class AutoMatchService
{
    private const double BasePixels = 1920.0 * 1080.0;
    private const double BaseFps = 60.0;

    public static int ComputeAutoTargetKbps(ProbeInfo meta, TargetCodec codec, AutoMatchConfig cfg)
    {
        return ComputeAutoTarget(meta, codec, cfg).TargetKbps;
    }

    public static AutoMatchResult ComputeAutoTarget(ProbeInfo meta, TargetCodec codec, AutoMatchConfig cfg)
    {
        var sourceKbps = GetSourceKbps(meta);
        if (sourceKbps <= 0)
        {
            return new AutoMatchResult(0, 0, 1);
        }

        var scale = ComputeScale(meta);
        var (balanced, safe) = GetMultipliers(codec);
        var t = Math.Clamp(cfg.Bias / 100.0, 0.0, 1.0);
        var multiplier = balanced + (safe - balanced) * t;
        var rawTarget = sourceKbps * multiplier * scale;
        var rounded = (int)Math.Round(rawTarget / 100.0, MidpointRounding.AwayFromZero) * 100;
        var min = Math.Max(800, (int)Math.Round(sourceKbps * 0.25));
        var max = 10 * sourceKbps;
        var target = Math.Clamp(rounded, min, max);

        return new AutoMatchResult(target, sourceKbps, scale);
    }

    public static void DebugValidateScale()
    {
        var scale1080p60 = ComputeScale(new ProbeInfo { Width = 1920, Height = 1080, Fps = 60 });
        Debug.Assert(scale1080p60 >= 0.85 && scale1080p60 <= 1.25);
        var scale4k60 = ComputeScale(new ProbeInfo { Width = 3840, Height = 2160, Fps = 60 });
        Debug.Assert(scale4k60 >= 0.85 && scale4k60 <= 1.25);
        var scale1080p30 = ComputeScale(new ProbeInfo { Width = 1920, Height = 1080, Fps = 30 });
        Debug.Assert(scale1080p30 >= 0.85 && scale1080p30 <= 1.25);
    }

    private static int GetSourceKbps(ProbeInfo meta)
    {
        if (meta.VideoBitrateKbps.HasValue && meta.VideoBitrateKbps.Value > 0)
        {
            return meta.VideoBitrateKbps.Value;
        }

        if (meta.OverallBitrateKbps.HasValue && meta.OverallBitrateKbps.Value > 0)
        {
            return meta.OverallBitrateKbps.Value;
        }

        return 0;
    }

    private static double ComputeScale(ProbeInfo meta)
    {
        var width = meta.Width > 0 ? meta.Width : 1920;
        var height = meta.Height > 0 ? meta.Height : 1080;
        var fps = meta.Fps > 0 ? meta.Fps : 60;
        var c = (width * height * fps) / (BasePixels * BaseFps);
        var scale = Math.Pow(Math.Max(c, 0.25), 0.15);
        return Math.Clamp(scale, 0.85, 1.25);
    }

    private static (double Balanced, double Safe) GetMultipliers(TargetCodec codec)
    {
        return codec == TargetCodec.H264
            ? (2.2, 2.8)
            : (1.35, 1.6);
    }
}

using System.Diagnostics;
using System.Text;

namespace AplysiaAv1Transcoder.Services;

public sealed class EncoderCapabilities
{
    public bool HasNvencH264 { get; set; }
    public bool HasNvencH265 { get; set; }
    public bool HasQsvH264 { get; set; }
    public bool HasQsvH265 { get; set; }
    public bool HasAmfH264 { get; set; }
    public bool HasAmfH265 { get; set; }
    public bool HasLibDav1d { get; set; }
}

public sealed class EncoderCapabilitiesService
{
    private string? _cachedPath;
    private EncoderCapabilities? _cached;

    public async Task<EncoderCapabilities> GetCapabilitiesAsync(string ffmpegPath)
    {
        if (_cached != null && string.Equals(_cachedPath, ffmpegPath, StringComparison.OrdinalIgnoreCase))
        {
            return _cached;
        }

        var encoderOutput = await RunFfmpegListAsync(ffmpegPath, "-hide_banner -encoders");
        var decoderOutput = await RunFfmpegListAsync(ffmpegPath, "-hide_banner -decoders");
        var capabilities = new EncoderCapabilities
        {
            HasNvencH264 = encoderOutput.Contains("h264_nvenc", StringComparison.OrdinalIgnoreCase),
            HasNvencH265 = encoderOutput.Contains("hevc_nvenc", StringComparison.OrdinalIgnoreCase),
            HasQsvH264 = encoderOutput.Contains("h264_qsv", StringComparison.OrdinalIgnoreCase),
            HasQsvH265 = encoderOutput.Contains("hevc_qsv", StringComparison.OrdinalIgnoreCase),
            HasAmfH264 = encoderOutput.Contains("h264_amf", StringComparison.OrdinalIgnoreCase),
            HasAmfH265 = encoderOutput.Contains("hevc_amf", StringComparison.OrdinalIgnoreCase),
            HasLibDav1d = decoderOutput.Contains("libdav1d", StringComparison.OrdinalIgnoreCase)
        };

        _cachedPath = ffmpegPath;
        _cached = capabilities;
        return capabilities;
    }

    private static async Task<string> RunFfmpegListAsync(string ffmpegPath, string arguments)
    {
        var psi = new ProcessStartInfo
        {
            FileName = ffmpegPath,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardOutputEncoding = System.Text.Encoding.UTF8,
            StandardErrorEncoding = System.Text.Encoding.UTF8
        };

        using var process = new Process { StartInfo = psi };
        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        return output + Environment.NewLine + error;
    }
}

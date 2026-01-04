using System.IO.Compression;
using AplysiaAv1Transcoder.Models;

namespace AplysiaAv1Transcoder.Services;

public sealed class FfmpegLocator
{
    public const string DefaultDownloadUrl = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-full.zip";

    private readonly StorageService _storage;
    private readonly HttpClient _httpClient;

    public FfmpegLocator(StorageService storage)
    {
        _storage = storage;
        _httpClient = new HttpClient();
    }

    public async Task<bool> ResolveAsync(AppSettings settings, IWin32Window owner)
    {
        if (TryResolveCandidate(Path.Combine(_storage.AppFolder, "ffmpeg", "bin", "ffmpeg.exe"), out var ffprobePath))
        {
            ApplyResolved(settings, Path.Combine(_storage.AppFolder, "ffmpeg", "bin", "ffmpeg.exe"), ffprobePath);
            return true;
        }

        if (!string.IsNullOrWhiteSpace(settings.FfmpegPath))
        {
            var stored = _storage.ResolvePath(settings.FfmpegPath);
            if (TryResolveCandidate(stored, out ffprobePath))
            {
                ApplyResolved(settings, stored, ffprobePath);
                return true;
            }
        }

        using var dialog = new FfmpegSetupForm(DefaultDownloadUrl);
        var result = dialog.ShowDialog(owner);
        if (result != DialogResult.OK)
        {
            return false;
        }

        if (dialog.Mode == FfmpegSetupMode.Browse)
        {
            if (!TryResolveCandidate(dialog.SelectedFfmpegPath, out ffprobePath))
            {
                MessageBox.Show(owner, "The selected ffmpeg.exe is invalid or ffprobe.exe was not found in the same folder.",
                    "FFmpeg", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            ApplyResolved(settings, dialog.SelectedFfmpegPath!, ffprobePath);
            return true;
        }

        if (dialog.Mode == FfmpegSetupMode.Download)
        {
            try
            {
                var url = dialog.DownloadUrl;
                var targetRoot = _storage.CanWriteTo(_storage.AppFolder) ? _storage.AppFolder : _storage.DataFolder;
                var downloaded = await DownloadAndExtractAsync(url, targetRoot);
                if (downloaded == null)
                {
                    MessageBox.Show(owner, "FFmpeg download completed but binaries were not found.",
                        "FFmpeg", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                ApplyResolved(settings, downloaded.Value.ffmpegPath, downloaded.Value.ffprobePath);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(owner, $"FFmpeg download failed: {ex.Message}", "FFmpeg", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        return false;
    }

    public bool IsValid(AppSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.ResolvedFfmpegPath))
        {
            return false;
        }

        return TryResolveCandidate(settings.ResolvedFfmpegPath, out _);
    }

    public bool TrySetFromPath(AppSettings settings, string ffmpegPath)
    {
        if (!TryResolveCandidate(ffmpegPath, out var ffprobePath))
        {
            return false;
        }

        ApplyResolved(settings, ffmpegPath, ffprobePath);
        return true;
    }

    private void ApplyResolved(AppSettings settings, string ffmpegPath, string ffprobePath)
    {
        settings.ResolvedFfmpegPath = ffmpegPath;
        settings.ResolvedFfprobePath = ffprobePath;
        settings.FfmpegPath = _storage.MakeRelativeIfInAppFolder(ffmpegPath);
    }

    private bool TryResolveCandidate(string? ffmpegPath, out string ffprobePath)
    {
        ffprobePath = string.Empty;
        if (string.IsNullOrWhiteSpace(ffmpegPath))
        {
            return false;
        }

        if (!File.Exists(ffmpegPath))
        {
            return false;
        }

        var dir = Path.GetDirectoryName(ffmpegPath);
        if (string.IsNullOrWhiteSpace(dir))
        {
            return false;
        }

        var candidate = Path.Combine(dir, "ffprobe.exe");
        if (!File.Exists(candidate))
        {
            return false;
        }

        ffprobePath = candidate;
        return true;
    }

    private async Task<(string ffmpegPath, string ffprobePath)?> DownloadAndExtractAsync(string url, string targetRoot)
    {
        var zipPath = Path.Combine(Path.GetTempPath(), $"ffmpeg_{Guid.NewGuid():N}.zip");
        await using (var output = File.Create(zipPath))
        {
            using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            await using var input = await response.Content.ReadAsStreamAsync();
            await input.CopyToAsync(output);
        }

        var extractRoot = Path.Combine(targetRoot, "ffmpeg");
        Directory.CreateDirectory(extractRoot);
        ZipFile.ExtractToDirectory(zipPath, extractRoot, true);
        File.Delete(zipPath);

        var ffmpeg = FindFirstBinary(extractRoot, "ffmpeg.exe");
        var ffprobe = FindFirstBinary(extractRoot, "ffprobe.exe");
        if (ffmpeg == null || ffprobe == null)
        {
            return null;
        }

        return (ffmpeg, ffprobe);
    }

    private static string? FindFirstBinary(string root, string fileName)
    {
        var matches = Directory.GetFiles(root, fileName, SearchOption.AllDirectories);
        if (matches.Length == 0)
        {
            return null;
        }

        var preferred = matches.FirstOrDefault(path => path.Contains("\\bin\\", StringComparison.OrdinalIgnoreCase));
        return preferred ?? matches[0];
    }
}

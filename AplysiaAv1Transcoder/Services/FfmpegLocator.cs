using System.Diagnostics;
using System.IO.Compression;
using AplysiaAv1Transcoder.Models;

namespace AplysiaAv1Transcoder.Services;

public sealed class FfmpegLocator
{
    public const string DefaultDownloadUrl = "https://github.com/BtbN/FFmpeg-Builds/releases/latest";

    private readonly StorageService _storage;
    private readonly HttpClient _httpClient;
    private readonly FfmpegDownloadService _downloadService;

    public FfmpegLocator(StorageService storage)
    {
        _storage = storage;
        _httpClient = new HttpClient();
        _downloadService = new FfmpegDownloadService(_httpClient);
    }

    public async Task<bool> ResolveAsync(AppSettings settings, IWin32Window owner, Action<LogEntry>? log = null, Action<string>? statusUpdate = null)
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
                var installRoot = Path.Combine(_storage.AppFolder, "ffmpeg");
                if (!_storage.CanWriteTo(_storage.AppFolder))
                {
                    MessageBox.Show(owner, "The application folder is not writable. Please run as administrator or download FFmpeg manually.",
                        "FFmpeg", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                if (Directory.Exists(installRoot))
                {
                    var overwrite = MessageBox.Show(owner,
                        "The .\\ffmpeg folder already exists. Overwrite it?",
                        "FFmpeg",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);
                    if (overwrite == DialogResult.Cancel || overwrite == DialogResult.No)
                    {
                        statusUpdate?.Invoke("FFmpeg download skipped.");
                        return false;
                    }

                    Directory.Delete(installRoot, true);
                }

                var downloaded = await DownloadAndExtractAsync(installRoot, log, statusUpdate);
                if (downloaded == null)
                {
                    statusUpdate?.Invoke("FFmpeg: Failed");
                    MessageBox.Show(owner, "FFmpeg download completed but binaries were not found.",
                        "FFmpeg", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                ApplyResolved(settings, downloaded.Value.ffmpegPath, downloaded.Value.ffprobePath);
                statusUpdate?.Invoke("FFmpeg: Done");
                return true;
            }
            catch (Exception ex)
            {
                statusUpdate?.Invoke("FFmpeg: Failed");
                log?.Invoke(new LogEntry { Level = LogLevel.Error, Message = $"FFmpeg download failed: {ex.Message}" });
                ShowDownloadFailure(owner, ex.Message);
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

    private async Task<(string ffmpegPath, string ffprobePath)?> DownloadAndExtractAsync(string installRoot, Action<LogEntry>? log, Action<string>? statusUpdate)
    {
        statusUpdate?.Invoke("FFmpeg: Fetching release info...");
        var asset = await _downloadService.GetLatestWin64GplAssetAsync();
        log?.Invoke(new LogEntry { Level = LogLevel.Info, Message = $"FFmpeg asset selected: {asset.Name}" });
        log?.Invoke(new LogEntry { Level = LogLevel.Info, Message = $"FFmpeg download URL: {asset.DownloadUrl}" });

        statusUpdate?.Invoke("FFmpeg: Downloading...");
        var zipPath = Path.Combine(Path.GetTempPath(), $"ffmpeg_{Guid.NewGuid():N}.zip");
        var lastLogged = -10;
        var progress = new Progress<int>(percent =>
        {
            statusUpdate?.Invoke($"FFmpeg: Downloading... {percent}%");
            if (percent >= lastLogged + 10 || percent == 100)
            {
                lastLogged = percent;
                log?.Invoke(new LogEntry { Level = LogLevel.Info, Message = $"FFmpeg download {percent}%" });
            }
        });
        await _downloadService.DownloadAssetAsync(asset.DownloadUrl, zipPath, progress);

        statusUpdate?.Invoke("FFmpeg: Extracting...");
        Directory.CreateDirectory(installRoot);
        ZipFile.ExtractToDirectory(zipPath, installRoot, true);
        File.Delete(zipPath);

        var ffmpeg = FindFirstBinary(installRoot, "ffmpeg.exe");
        var ffprobe = FindFirstBinary(installRoot, "ffprobe.exe");
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

    private static void ShowDownloadFailure(IWin32Window owner, string reason)
    {
        var page = new TaskDialogPage
        {
            Caption = "FFmpeg",
            Heading = "Automatic FFmpeg download failed",
            Text = $"Automatic FFmpeg download failed: {reason}. You can download manually from https://github.com/BtbN/FFmpeg-Builds/releases/latest and then select ffmpeg.exe."
        };
        var openButton = new TaskDialogButton("Open download page");
        page.Buttons.Add(openButton);
        page.Buttons.Add(TaskDialogButton.Close);

        var result = TaskDialog.ShowDialog(owner, page);
        if (result == openButton)
        {
            Process.Start(new ProcessStartInfo(DefaultDownloadUrl) { UseShellExecute = true });
        }
    }
}

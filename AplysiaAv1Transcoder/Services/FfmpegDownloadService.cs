using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace AplysiaAv1Transcoder.Services;

public sealed class FfmpegDownloadService
{
    private const string ReleaseApiUrl = "https://api.github.com/repos/BtbN/FFmpeg-Builds/releases/latest";
    private readonly HttpClient _httpClient;

    public FfmpegDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        if (!_httpClient.DefaultRequestHeaders.UserAgent.Any())
        {
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("AplysiaAv1Transcoder", "1.0"));
        }
        if (!_httpClient.DefaultRequestHeaders.Accept.Any(h => h.MediaType == "application/vnd.github+json"))
        {
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        }
    }

    public async Task<(string Name, string DownloadUrl)> GetLatestWin64GplAssetAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(ReleaseApiUrl, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new FfmpegDownloadException($"GitHub API request failed with HTTP {(int)response.StatusCode} {response.ReasonPhrase}.", response.StatusCode);
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        if (!doc.RootElement.TryGetProperty("assets", out var assets))
        {
            throw new FfmpegDownloadException("GitHub API response did not include assets.");
        }

        var matches = new List<(string name, string url)>();
        foreach (var asset in assets.EnumerateArray())
        {
            if (!asset.TryGetProperty("name", out var nameProp) ||
                !asset.TryGetProperty("browser_download_url", out var urlProp))
            {
                continue;
            }

            var name = nameProp.GetString() ?? string.Empty;
            var url = urlProp.GetString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(url))
            {
                continue;
            }

            var lower = name.ToLowerInvariant();
            if (!lower.Contains("win64") || !lower.Contains("gpl") || !lower.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            matches.Add((name, url));
        }

        if (matches.Count == 0)
        {
            throw new FfmpegDownloadException("No win64 GPL ZIP assets found in GitHub release.");
        }

        var preferred = matches.FirstOrDefault(a => string.Equals(a.name, "ffmpeg-master-latest-win64-gpl.zip", StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(preferred.name))
        {
            return (preferred.name, preferred.url);
        }

        var best = matches.OrderBy(a => a.name.Length).First();
        return (best.name, best.url);
    }

    public async Task DownloadAssetAsync(string downloadUrl, string destinationPath, IProgress<int>? progress = null, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new FfmpegDownloadException($"FFmpeg download failed with HTTP {(int)response.StatusCode} {response.ReasonPhrase}.", response.StatusCode);
        }

        var total = response.Content.Headers.ContentLength;
        await using var input = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var output = File.Create(destinationPath);

        var buffer = new byte[81920];
        long totalRead = 0;
        int read;
        while ((read = await input.ReadAsync(buffer, cancellationToken)) > 0)
        {
            await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            if (total.HasValue && total.Value > 0)
            {
                totalRead += read;
                var percent = (int)Math.Clamp((totalRead * 100L) / total.Value, 0, 100);
                progress?.Report(percent);
            }
        }

        progress?.Report(100);
    }
}

public sealed class FfmpegDownloadException : Exception
{
    public HttpStatusCode? StatusCode { get; }

    public FfmpegDownloadException(string message, HttpStatusCode? statusCode = null, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}

namespace AplysiaAv1Transcoder.Services;

public interface IVideoDurationProvider
{
    Task<TimeSpan?> TryGetDurationAsync(string filePath, CancellationToken ct);
}

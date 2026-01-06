using System.Runtime.InteropServices;
using AplysiaAv1Transcoder.Models;
using AplysiaAv1Transcoder.Services.Interop;

namespace AplysiaAv1Transcoder.Services;

public sealed class MediaFoundationDurationProvider : IVideoDurationProvider
{
    private static readonly Guid MfPdDuration = new("6C990D33-BB8E-477A-859D-0D5D96FCD88A");
    private readonly Action<LogEntry>? _log;

    public MediaFoundationDurationProvider(Action<LogEntry>? log = null)
    {
        _log = log;
    }

    public Task<TimeSpan?> TryGetDurationAsync(string filePath, CancellationToken ct)
    {
        return Task.Run(() =>
        {
            ct.ThrowIfCancellationRequested();
            var init = NativeMethods.CoInitializeEx(IntPtr.Zero, NativeMethods.COINIT.MULTITHREADED);
            var canUninit = init == NativeMethods.S_OK || init == NativeMethods.S_FALSE;

            try
            {
                NativeMethods.MFStartup(0x20070, 0);
                NativeMethods.MFCreateSourceReaderFromURL(filePath, IntPtr.Zero, out var reader);
                var durationKey = MfPdDuration;
                reader.GetPresentationAttribute(-1, ref durationKey, out var value);
                var duration100Ns = value.GetLong();
                NativeMethods.PropVariantClear(ref value);
                Marshal.ReleaseComObject(reader);

                if (duration100Ns <= 0)
                {
                    _log?.Invoke(new LogEntry { Level = LogLevel.Info, Message = "Media Foundation duration unavailable" });
                    return (TimeSpan?)null;
                }

                var duration = TimeSpan.FromTicks(duration100Ns);
                _log?.Invoke(new LogEntry { Level = LogLevel.Info, Message = $"Duration via Media Foundation: {duration}" });
                return (TimeSpan?)duration;
            }
            catch (Exception ex)
            {
                _log?.Invoke(new LogEntry { Level = LogLevel.Info, Message = $"Media Foundation duration unavailable ({ex.Message})" });
                return (TimeSpan?)null;
            }
            finally
            {
                try
                {
                    NativeMethods.MFShutdown();
                }
                catch
                {
                }

                if (canUninit)
                {
                    NativeMethods.CoUninitialize();
                }
            }
        }, ct);
    }
}

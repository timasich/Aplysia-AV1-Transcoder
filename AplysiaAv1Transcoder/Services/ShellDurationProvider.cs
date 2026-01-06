using System.Runtime.InteropServices;
using AplysiaAv1Transcoder.Models;
using AplysiaAv1Transcoder.Services.Interop;

namespace AplysiaAv1Transcoder.Services;

public sealed class ShellDurationProvider : IVideoDurationProvider
{
    private static readonly Guid PropertyStoreGuid = new("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99");
    private static readonly PropertyKey DurationKey = new(new Guid("64440490-4C8B-11D1-8B70-080036B11A03"), 3);
    private readonly Action<LogEntry>? _log;

    public ShellDurationProvider(Action<LogEntry>? log = null)
    {
        _log = log;
    }

    public Task<TimeSpan?> TryGetDurationAsync(string filePath, CancellationToken ct)
    {
        return Task.Run(() =>
        {
            ct.ThrowIfCancellationRequested();
            var init = NativeMethods.CoInitializeEx(IntPtr.Zero, NativeMethods.COINIT.APARTMENTTHREADED);
            var canUninit = init == NativeMethods.S_OK || init == NativeMethods.S_FALSE;

            try
            {
                var propertyStoreGuid = PropertyStoreGuid;
                var hr = NativeMethods.SHGetPropertyStoreFromParsingName(filePath, IntPtr.Zero, NativeMethods.GPS_DEFAULT, ref propertyStoreGuid, out var store);
                if (hr != NativeMethods.S_OK)
                {
                    _log?.Invoke(new LogEntry { Level = LogLevel.Info, Message = "Shell duration unavailable" });
                    return (TimeSpan?)null;
                }

                var durationKey = DurationKey;
                var hrValue = store.GetValue(ref durationKey, out var value);
                if (hrValue != NativeMethods.S_OK)
                {
                    _log?.Invoke(new LogEntry { Level = LogLevel.Info, Message = "Shell duration unavailable" });
                    Marshal.ReleaseComObject(store);
                    return (TimeSpan?)null;
                }

                var duration100Ns = (long)value.GetULong();
                NativeMethods.PropVariantClear(ref value);
                Marshal.ReleaseComObject(store);

                if (duration100Ns <= 0)
                {
                    _log?.Invoke(new LogEntry { Level = LogLevel.Info, Message = "Shell duration unavailable" });
                    return (TimeSpan?)null;
                }

                var duration = TimeSpan.FromTicks(duration100Ns);
                _log?.Invoke(new LogEntry { Level = LogLevel.Info, Message = $"Duration via Shell: {duration}" });
                return (TimeSpan?)duration;
            }
            catch (Exception ex)
            {
                _log?.Invoke(new LogEntry { Level = LogLevel.Info, Message = $"Shell duration unavailable ({ex.Message})" });
                return (TimeSpan?)null;
            }
            finally
            {
                if (canUninit)
                {
                    NativeMethods.CoUninitialize();
                }
            }
        }, ct);
    }
}

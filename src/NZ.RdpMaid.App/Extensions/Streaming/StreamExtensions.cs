using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NZ.RdpMaid.App.Extensions.Streaming;

internal static class StreamExtensions
{
    public static async Task CopyWithProgressTracking(
        this Stream source,
        Stream target,
        IProgress<float> progress,
        long contentLength,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(target, nameof(target));
        ArgumentNullException.ThrowIfNull(progress, nameof(progress));

        if (contentLength <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(contentLength));
        }

        IProgress<long> relativeProgress = new Progress<long>(
            byteCount => progress.Report((float)byteCount / contentLength)
        );

        var buf = new byte[2048];
        long totalBytesRead = 0;
        int bytesRead;

        while ((bytesRead = await source.ReadAsync(buf, ct)) != 0)
        {
            await target.WriteAsync(buf.AsMemory(0, bytesRead), ct);
            totalBytesRead += bytesRead;
            relativeProgress.Report(totalBytesRead);
        }
    }
}
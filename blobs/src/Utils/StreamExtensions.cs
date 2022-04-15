using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ED.Blobs
{
    public static class StreamExtensions
    {
        public static async Task<long> CopyToAsync(
            this Stream source,
            Stream destination,
            int bufferSize,
            CancellationToken ct)
        {
            return await CopyToAsync(
                source,
                destination,
                bufferSize,
                long.MaxValue,
                ct);
        }

        public static async Task<long> CopyToMultipleAsync(
            this Stream source,
            Stream[] destinations,
            int bufferSize,
            CancellationToken ct)
        {
            return await CopyToMultipleAsync(
                source,
                destinations,
                bufferSize,
                long.MaxValue,
                ct);
        }

        public static async Task<long> CopyToAsync(
            this Stream source,
            Stream destination,
            int bufferSize,
            long length,
            CancellationToken ct)
        {
            long bytesCopied = 0;
            long readSoFar = 0;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
            try
            {
                do
                {
                    int next = (int)Math.Min(length - readSoFar, buffer.Length);
                    int bytesRead =
                        await source
                            .ReadAsync(new Memory<byte>(buffer, 0, next), ct)
                            .ConfigureAwait(false);

                    if (bytesRead == 0)
                        break;

                    bytesCopied += bytesRead;
                    await destination
                        .WriteAsync(
                            new ReadOnlyMemory<byte>(buffer, 0, bytesRead),
                            ct)
                        .ConfigureAwait(false);

                    readSoFar += bytesRead;
                } while (readSoFar < length);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            return bytesCopied;
        }

        public static async Task<long> CopyToMultipleAsync(
            this Stream source,
            Stream[] destinations,
            int bufferSize,
            long length,
            CancellationToken ct)
        {
            long bytesCopied = 0;
            long readSoFar = 0;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
            try
            {
                do
                {
                    int next = (int)Math.Min(length - readSoFar, buffer.Length);
                    int bytesRead =
                        await source
                            .ReadAsync(new Memory<byte>(buffer, 0, next), ct)
                            .ConfigureAwait(false);

                    if (bytesRead == 0)
                        break;

                    bytesCopied += bytesRead;
                    foreach (var destination in destinations)
                    {
                        await destination
                            .WriteAsync(
                                new ReadOnlyMemory<byte>(buffer, 0, bytesRead),
                                ct)
                            .ConfigureAwait(false);
                    }

                    readSoFar += bytesRead;
                } while (readSoFar < length);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            return bytesCopied;
        }
    }
}

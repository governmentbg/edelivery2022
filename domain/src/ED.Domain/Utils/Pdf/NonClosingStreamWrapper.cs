using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    // base on NonClosingStreamWrapper class from Jon Skeet's MiscUtil library
    // https://jonskeet.uk/csharp/miscutil/

    /// <summary>
    /// Wraps a stream for all operations except Close and Dispose, which
    /// merely flush the stream and prevent further operations from being
    /// carried out using this wrapper.
    /// </summary>
    public sealed class NonClosingWrapperStream : Stream
    {
        #region Members specific to this wrapper class
        /// <summary>
        /// Creates a new instance of the class, wrapping the specified stream.
        /// </summary>
        /// <param name="stream">The stream to wrap. Must not be null.</param>
        /// <exception cref="ArgumentNullException">stream is null</exception>
        public NonClosingWrapperStream(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            this.stream = stream;
        }

#pragma warning disable CA2213 // Disposable fields should be disposed
        Stream stream;
#pragma warning restore CA2213 // Disposable fields should be disposed
        /// <summary>
        /// Stream wrapped by this wrapper
        /// </summary>
        public Stream BaseStream
        {
            get { return this.stream; }
        }

        /// <summary>
        /// Whether this stream has been closed or not
        /// </summary>
        bool closed;

        /// <summary>
        /// Throws an InvalidOperationException if the wrapper is closed.
        /// </summary>
        void CheckClosed()
        {
            if (this.closed)
            {
                throw new InvalidOperationException("Wrapper has been closed or disposed");
            }
        }
        #endregion

        #region Overrides of Stream methods and properties
        public override long Position
        {
            get
            {
                this.CheckClosed();
                return this.stream.Position;
            }
            set
            {
                this.CheckClosed();
                this.stream.Position = value;
            }
        }

        public override long Length
        {
            get
            {
                this.CheckClosed();
                return this.stream.Length;
            }
        }

        public override bool CanWrite
        {
            get { return this.closed ? false : this.stream.CanWrite; }
        }

        public override bool CanTimeout
        {
            get { return this.closed ? false : this.stream.CanTimeout; }
        }

        public override bool CanSeek
        {
            get { return this.closed ? false : this.stream.CanSeek; }
        }

        public override bool CanRead
        {
            get { return this.closed ? false : this.stream.CanRead; }
        }

        public override int ReadTimeout
        {
            get
            {
                this.CheckClosed();
                return this.stream.ReadTimeout;
            }
            set
            {
                this.CheckClosed();
                this.stream.ReadTimeout = value;
            }
        }

        public override int WriteTimeout
        {
            get
            {
                this.CheckClosed();
                return this.stream.WriteTimeout;
            }
            set
            {
                this.CheckClosed();
                this.stream.WriteTimeout = value;
            }
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        {
            this.CheckClosed();
            return this.stream.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        {
            this.CheckClosed();
            return this.stream.BeginWrite(buffer, offset, count, callback, state);
        }

        public override void Close()
        {
            if (!this.closed)
            {
                this.stream.Flush();
            }
            this.closed = true;
        }

        public override void CopyTo(Stream destination, int bufferSize)
        {
            this.CheckClosed();
            this.stream.CopyTo(destination, bufferSize);
        }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            this.CheckClosed();
            return this.stream.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            this.CheckClosed();
            return this.stream.EndRead(asyncResult);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            this.CheckClosed();
            this.stream.EndWrite(asyncResult);
        }

        public override void Flush()
        {
            this.CheckClosed();
            this.stream.Flush();
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            this.CheckClosed();
            return this.stream.FlushAsync(cancellationToken);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            this.CheckClosed();
            return this.stream.Read(buffer, offset, count);
        }

        public override int Read(Span<byte> buffer)
        {
            this.CheckClosed();
            return this.stream.Read(buffer);
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            this.CheckClosed();
            return this.stream.ReadAsync(buffer, cancellationToken);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            this.CheckClosed();
            return this.stream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override int ReadByte()
        {
            this.CheckClosed();
            return this.stream.ReadByte();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            this.CheckClosed();
            return this.stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            this.CheckClosed();
            this.stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.CheckClosed();
            this.stream.Write(buffer, offset, count);
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            this.CheckClosed();
            this.stream.Write(buffer);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            this.CheckClosed();
            return this.stream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            this.CheckClosed();
            return this.stream.WriteAsync(buffer, cancellationToken);
        }

        public override void WriteByte(byte value)
        {
            this.CheckClosed();
            this.stream.WriteByte(value);
        }
        #endregion
    }
}

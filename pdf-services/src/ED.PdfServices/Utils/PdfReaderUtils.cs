using iTextSharp.text.io;
using iTextSharp.text.pdf;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.X509;
using System;
using System.IO;
using System.Reflection;
using System.Threading;

namespace ED.PdfServices
{
    public static class PdfReaderUtils
    {
        sealed class MemoryStreamRandomAccessSource : IRandomAccessSource
        {
            private readonly MemoryStream stream;
            public MemoryStreamRandomAccessSource(MemoryStream stream)
            {
                this.stream = stream;
            }

            public long Length => this.stream.Length;

            public void Close()
            {
                this.stream.Close();
            }

            public void Dispose()
            {
                this.stream.Dispose();
            }

            public int Get(long position)
            {
                if (position >= this.stream.Length)
                {
                    return -1;
                }

                this.stream.Seek(position, SeekOrigin.Begin);
                return 0xff & this.stream.ReadByte();
            }

            public int Get(long position, byte[] bytes, int off, int len)
            {
                if (position >= this.stream.Length)
                {
                    return -1;
                }

                if (position + len > this.stream.Length)
                {
                    len = (int)(this.stream.Length - position);
                }

                this.stream.Seek(position, SeekOrigin.Begin);
                return this.stream.Read(bytes, off, len);
            }
        }

        private static Lazy<ConstructorInfo> PdfReaderConstructorInfo =
            new Lazy<ConstructorInfo>(
                () => typeof(PdfReader).GetConstructor(
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new Type[]
                    {
                        typeof(IRandomAccessSource),
                        typeof(bool),
                        typeof(byte[]),
                        typeof(X509Certificate),
                        typeof(ICipherParameters),
                        typeof(bool)
                    },
                    null),
                LazyThreadSafetyMode.ExecutionAndPublication);
      
        /// <summary>
        /// Creates a PdfReader without copying the memory in the stream.
        /// IMPORTANT!!! The memoryStream should be left unmodified and only disposed of
        /// after all work with this reader has been finished and disposed.
        /// </summary>
        /// <param name="memoryStream"></param>
        /// <param name="partialRead"></param>
        /// <returns></returns>
        public static PdfReader CreateFromMemoryStream(
            MemoryStream memoryStream,
            bool partialRead)
            => (PdfReader)PdfReaderConstructorInfo.Value.Invoke(
                new object[]
                {
#pragma warning disable CA2000 // Dispose objects before losing scope
                    new IndependentRandomAccessSource(
                        new MemoryStreamRandomAccessSource(memoryStream)),
#pragma warning restore CA2000 // Dispose objects before losing scope
                    partialRead,
                    null,
                    null,
                    null,
                    true
                });
    }
}
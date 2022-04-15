using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using iTextSharp.text.io;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.security;

namespace ED.Domain
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
                this.stream.Seek(position, SeekOrigin.Begin);
                return this.stream.ReadByte();
            }

            public int Get(long position, byte[] bytes, int off, int len)
            {
                this.stream.Seek(position, SeekOrigin.Begin);
                return this.stream.Read(bytes, off, len);
            }
        }

        private static Lazy<ConstructorInfo> PdfReaderConstructorInfo =
            new(
                () => typeof(PdfReader).GetConstructor(
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new Type[] { typeof(ReaderProperties), typeof(IRandomAccessSource) },
                    null)!,
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Creates a PdfReader without copying the memory in the stream.
        /// IMPORTANT!!! The memoryStream should be left unmodified and only disposed of
        /// after all work with this reader has been finished and disposed.
        /// </summary>
        /// <param name="memoryStream"></param>
        /// <returns></returns>
        public static PdfReader CreateFromMemoryStream(MemoryStream memoryStream)
            => (PdfReader)PdfReaderConstructorInfo.Value.Invoke(
                new object[]
                {
                    new ReaderProperties(),
#pragma warning disable CA2000 // Dispose objects before losing scope
                    new IndependentRandomAccessSource(
                        new MemoryStreamRandomAccessSource(memoryStream))
#pragma warning restore CA2000 // Dispose objects before losing scope
                });

        public static void FillForm(
            MemoryStream templateStream,
            MemoryStream outputStream,
            IDictionary<string, string> fieldValues)
        {
            using PdfReader reader =
                PdfReaderUtils.CreateFromMemoryStream(templateStream);
            using PdfStamper stamper =
                new(
                    reader,
                    // disposing the stamper disposes the stream
                    // which is something we dont wont
                    new NonClosingWrapperStream(outputStream),
                    '\0',
                    false);

            AcroFields fields = stamper.AcroFields;

            fields.GenerateAppearances = false;

            foreach (var item in fieldValues)
            {
                if (fields.Fields.ContainsKey(item.Key))
                {
                    fields.SetField(item.Key, item.Value);
                }
            }
        }

        public static IDictionary<string, string?> ExtractFormValues(
            MemoryStream pdfStream,
            string[] fields)
        {
            Dictionary<string, string?> result = new();

            using PdfReader reader = CreateFromMemoryStream(pdfStream);

            AcroFields acroFields = reader.AcroFields;

            System.Xml.XmlElement xfaDocNode = reader.AcroFields?.Xfa?.DatasetsNode?.OwnerDocument?.DocumentElement
                ?? throw new Exception("Cannot parse document");

            foreach (string field in fields)
            {
                string? match = acroFields.GetField(field);
                if (string.IsNullOrEmpty(match))
                {
                    match = xfaDocNode.SelectSingleNode("//" + field)?.InnerText;
                }

                if (!result.ContainsKey(field))
                {
                    result.Add(field, match);
                }
            }

            return result;
        }

        public static void Sign(
            MemoryStream pdfStream,
            MemoryStream outputStream,
            string signatureDocumentField,
            string reason,
            string location,
            string certificateStore,
            StoreLocation certificateStoreLocation,
            string certificateThumbprint)
        {
            using X509Certificate2? x509Certificate =
                X509Certificate2Utils.LoadX509Certificate(
                    certificateStore,
                    certificateStoreLocation,
                    certificateThumbprint)
                ?? throw new DomainException("Missing signing certificate");

            Org.BouncyCastle.X509.X509Certificate bouncyX509Certificates =
                Org.BouncyCastle.Security.DotNetUtilities.FromX509Certificate(x509Certificate);

            using RSA rsa = x509Certificate.GetRSAPrivateKey()!;

            Org.BouncyCastle.Crypto.AsymmetricCipherKeyPair keyPair =
                Org.BouncyCastle.Security.DotNetUtilities.GetKeyPair(rsa);

            IExternalSignature es = new PrivateKeySignature(
                keyPair.Private,
                "SHA-256");

            using PdfReader reader = CreateFromMemoryStream(pdfStream);

            // not using a "using" for the stamper as disposing it
            // before the signature has been closed will throw an exception
            // and hide the exception that prevented the signature creation
            PdfStamper stamper = PdfStamper.CreateSignature(
                reader,
                // closing the stamper disposes the stream
                // which is something we dont wont
                new NonClosingWrapperStream(outputStream),
                '\0',
                null,
                false);

            PdfSignatureAppearance appearance = stamper.SignatureAppearance;
            appearance.Reason = reason;
            appearance.Location = location;
            appearance.SetVisibleSignature(signatureDocumentField);

            MakeSignature.SignDetached(
                appearance,
                es,
                new Org.BouncyCastle.X509.X509Certificate[]
                {
                    bouncyX509Certificates
                },
                null,
                null,
                null,
                0,
                CryptoStandard.CMS);

            stamper.Close();
        }
    }
}

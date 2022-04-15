using iTextSharp.text.exceptions;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.security;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace ED.PdfServices
{
    public class SignatureInfo
    {
        public byte[] SigningCertificate { get; set; }
        public bool CoversDocument { get; set; }
        public bool CoversPriorRevision { get; set; }
        public bool IsTimestamp { get; set; }
        public DateTime SignDate { get; set; }
        public bool ValidAtTimeOfSigning { get; set; }
        public string Issuer { get; set; }
        public string Subject { get; set; }
        public string SerialNumber { get; set; }
        public int Version { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
    }

    public class BlobInfo
    { 
        public string FileName { get; set; }
        public int BlobId { get; set; }
    }

    [RoutePrefix("api/signatures")]
    public class SignaturesController : ApiController
    {
        private readonly ILogger<SignaturesController> logger;
        private readonly RecyclableMemoryStreamManager recyclableMemoryStreamManager;

        public SignaturesController(
            ILogger<SignaturesController> logger,
            RecyclableMemoryStreamManager recyclableMemoryStreamManager)
        {
            this.logger = logger;
            this.recyclableMemoryStreamManager = recyclableMemoryStreamManager;
        }

        [Route("extract")]
        [HttpPost]
        public async Task<IEnumerable<SignatureInfo>> ExtractAsync(CancellationToken ct)
        {
            using (MemoryStream ms = this.recyclableMemoryStreamManager.GetStream())
            {
                BlobInfo blobInfo;
                try
                {
                    blobInfo = await ReadContentAsync(
                        this.Request.Content,
                        ms,
                        ct);
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "Reading multipart content failed");
                    throw new HttpResponseException(HttpStatusCode.BadRequest);
                }

                var signatures = new List<SignatureInfo>();

                try
                {
                    foreach (var signature in ExtractPdfSignatures(ms))
                    {
                        (byte[] signingCertificate,
                        bool coversDocument,
                        bool coversPriorRevision,
                        bool isTimestamp,
                        DateTime signDate) = signature;

                        (bool validAtTimeOfSigning,
                        string issuer,
                        string subject,
                        string serialNumber,
                        int version,
                        DateTime validFrom,
                        DateTime validTo) = ExtractSignatureInfo(signingCertificate, signDate);

                        signatures.Add(
                            new SignatureInfo
                            {
                                SigningCertificate = signingCertificate,
                                CoversDocument = coversDocument,
                                CoversPriorRevision = coversPriorRevision,
                                IsTimestamp = isTimestamp,
                                SignDate = signDate,
                                ValidAtTimeOfSigning = validAtTimeOfSigning,
                                Issuer = issuer,
                                Subject = subject,
                                SerialNumber = serialNumber,
                                Version = version,
                                ValidFrom = validFrom,
                                ValidTo = validTo,
                            });
                    }

                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, $"Extracting pdf signatures failed for fileName: '{blobInfo.FileName}', blobId: {blobInfo.BlobId}");
                    throw new HttpResponseException(HttpStatusCode.InternalServerError);
                }

                this.logger.LogInformation($"Extracted {signatures.Count} signatures for fileName: '{blobInfo.FileName}', blobId: {blobInfo.BlobId}");
                return signatures;
            }
        }

        private const int DefaultCopyBufferSize = 81920;
        private static async Task<BlobInfo> ReadContentAsync(
            HttpContent content,
            MemoryStream pdfStream,
            CancellationToken ct)
        {
            if (!content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var provider = new MultipartMemoryStreamProvider();
            await content.ReadAsMultipartAsync(provider, ct);

            bool hasPdfContent = false;
            BlobInfo blobInfo = null;

            foreach (var part in provider.Contents)
            {
                var contentDisposition = part.Headers.ContentDisposition;
                string name = contentDisposition.Name;

                if (name == "pdf")
                {
                    hasPdfContent = true;
                    var contentStream = await part.ReadAsStreamAsync();
                    await contentStream.CopyToAsync(pdfStream, DefaultCopyBufferSize, ct);
                }
                else if (name == "blobInfo")
                {
                    string contentString = await part.ReadAsStringAsync();
                    using (StringReader reader = new StringReader(contentString))
                    using (JsonTextReader jsonReader = new JsonTextReader(reader))
                    {
                        JsonSerializer jsonSerializer = new JsonSerializer();
                        blobInfo = jsonSerializer.Deserialize<BlobInfo>(jsonReader);
                    }
                }
                else
                {
                    throw new Exception("Unexpected content");
                }
            }

            if (!hasPdfContent)
            {
                throw new Exception("The multipart content is missing a 'pdf' part.");
            }
            if (blobInfo == null)
            {
                throw new Exception("The multipart content is missing a 'blobInfo' part.");
            }

            return blobInfo;
        }

        private static IEnumerable<(
            byte[] signingCertificate,
            bool coversDocument,
            bool coversPriorRevision,
            bool isTimestamp,
            DateTime signDate)> ExtractPdfSignatures(MemoryStream pdfStream)
        {
            var signatures =
                new List<(
                    byte[] signingCertificate,
                    bool coversDocument,
                    bool coversPriorRevision,
                    bool isTimestamp,
                    DateTime signDate)>();

            PdfReader pdf_;
            try
            {
                pdf_ = PdfReaderUtils.CreateFromMemoryStream(pdfStream, true);
            }
            catch (TargetInvocationException tie) when (tie.InnerException is InvalidPdfException)
            {
                // ignore broken pdfs
                return signatures;
            }

            using (PdfReader pdf = pdf_)
            {
                var signatureNames = pdf.AcroFields.GetSignatureNames();

                foreach (var signatureName in signatureNames)
                {
                    bool coversDoc;
                    bool coversPriorRevision;
                    PdfPKCS7 signature;

                    int revision = pdf.AcroFields.GetRevision(signatureName);
                    if (revision != pdf.AcroFields.TotalRevisions)
                    {
                        // get the signature from the revision it is applied to
                        try
                        {
                            using (PdfReader pdfAtRevision = new PdfReader(pdf.AcroFields.ExtractRevision(signatureName)))
                            {
                                (coversDoc, signature) = ExtractSignature(pdfAtRevision, signatureName);
                                coversPriorRevision = true;
                            }
                        }
                        catch (InvalidPdfException)
                        {
                            // carried over from old project
                            // when we can't get the revision, treat it as the signature doesn't cover it 
                            coversDoc = false;
                            coversPriorRevision = false;
                            (_, signature) = ExtractSignature(pdf, signatureName);
                        }
                    }
                    else
                    {
                        (coversDoc, signature) = ExtractSignature(pdf, signatureName);
                        coversPriorRevision = false;
                    }

                    signatures.Add((
                        signature.SigningCertificate.GetEncoded(),
                        coversDoc && signature.Verify(),
                        coversPriorRevision,
                        signature.IsTsp,
                        // Tsp or Ltv (see https://www.mail-archive.com/itext-questions@lists.sourceforge.net/msg59504.html)
                        signature.IsTsp || signature.SignDate == DateTime.MinValue
                            ? signature.TimeStampDate
                            : signature.SignDate));
                }
            }

            return signatures;
        }

        private static (bool coversDoc, PdfPKCS7 signature) ExtractSignature(PdfReader pdf, string signatureName)
        {
            return (
                coversDoc: pdf.AcroFields.SignatureCoversWholeDocument(signatureName),
                signature: pdf.AcroFields.VerifySignature(signatureName)
            );
        }

        private static (
            bool validAtTimeOfSigning,
            string issuer,
            string subject,
            string serialNumber,
            int version,
            DateTime validFrom,
            DateTime validTo) ExtractSignatureInfo(
                byte[] signingCertificate,
                DateTime signDate)
        {
            using (X509Certificate2 cert = new X509Certificate2(signingCertificate))
            using (X509Chain chain = new X509Chain())
            {
                chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;
                chain.ChainPolicy.VerificationTime = signDate;

                bool validAtTimeOfSigning = chain.Build(cert);
                string issuer = cert.Issuer;
                string subject = cert.Subject;
                string serialNumber = cert.SerialNumber;
                int version = cert.Version;
                DateTime validFrom = cert.NotBefore;
                DateTime validTo = cert.NotAfter;

                return (
                    validAtTimeOfSigning,
                    issuer,
                    subject,
                    serialNumber,
                    version,
                    validFrom,
                    validTo);
            }
        }
    }
}

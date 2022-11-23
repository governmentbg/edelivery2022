using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using static ED.Domain.IMessageOpenHORepository;
using static ED.Domain.IProfilesService;

namespace ED.Domain
{
    partial class MessageOpenHORepository : IMessageOpenHORepository
    {
        private const string AsSender_PdfFileName = "Удостоверение за връчване-{0}_{1}.pdf";
        private const string AsSender_PdfContentType = "application/pdf";
        private const string AsSender_SignatureReason = "Non-Repudiation of Origin";
        private const string AsSender_SignatureLocation = "Bulgaria, Sofia";

        public async Task<GetPdfAsSenderVO> GetPdfAsSenderAsync(
            int messageId,
            CancellationToken ct)
        {
            IMessageOpenQueryRepository.GetPdfAsSenderVO pdfAsSender =
                await this.messageOpenQueryRepository.GetPdfAsSenderAsync(
                    messageId,
                    ct);

            // for legacy messages
            if (pdfAsSender.MessagePdfBlobId.HasValue)
            {
                // TODO: maybe tell the consumer to download the blob himself
                // instead of proxying the result inefficiently
                BlobsServiceClient.DownloadBlobToArrayVO blob =
                    await this.blobsServiceClient.DownloadProfileBlobToArrayAsync(
                        pdfAsSender.MessagePdfBlobId.Value,
                        pdfAsSender.SenderProfileId,
                        ct);

                return new GetPdfAsSenderVO(
                    blob.FileName,
                    AsSender_PdfContentType,
                    blob.Content);
            }

            IMessageOpenQueryRepository.GetMessageAccessKeyVO messageAccessKey =
                await this.messageOpenQueryRepository.GetMessageAccessKeyAsync(
                    messageId,
                    pdfAsSender.SenderProfileId,
                    ct);

            ProfileKeyVO profileKey =
              await this.profilesService.GetProfileKeyAsync(
                  messageAccessKey.ProfileKeyId,
                  ct);

            Keystore.DecryptWithRsaKeyResponse decryptedKeyResp =
              await this.keystoreClient.DecryptWithRsaKeyAsync(
                  request: new Keystore.DecryptWithRsaKeyRequest
                  {
                      Key = new Keystore.RsaKey
                      {
                          Provider = profileKey.Provider,
                          KeyName = profileKey.KeyName,
                          OaepPadding = profileKey.OaepPadding,
                      },
                      EncryptedData = ByteString.CopyFrom(messageAccessKey.EncryptedKey)
                  },
                  cancellationToken: ct);

            IEncryptor encryptor = this.encryptorFactory.CreateEncryptor(
                decryptedKeyResp.Plaintext.ToByteArray(),
                pdfAsSender.IV);

            string decryptedBody =
                Encoding.UTF8.GetString(encryptor.Decrypt(pdfAsSender.Body));

            IMessageOpenQueryRepository.GetTemplateContentVO template =
                await this.messageOpenQueryRepository.GetTemplateContentAsync(
                    pdfAsSender.TemplateId!.Value,
                    ct);

            using MemoryStream newPdf =
                this.recyclableMemoryStreamManager.GetStream();

            byte[] messageSummarySha256;
            if (pdfAsSender.MessageSummaryVersion == MessageSummaryVersion.V1)
            {
                using IEncryptor encryptorV1 = this.encryptorFactoryV1.CreateEncryptor();
                messageSummarySha256 = SHA256.HashData(
                    encryptorV1.Decrypt(
                        pdfAsSender.MessageSummary
                        ?? throw new Exception("MessageSummary should not be null")));
            }
            else if (pdfAsSender.MessageSummaryVersion == MessageSummaryVersion.V2)
            {
                using MemoryStream memoryStream = new(
                    pdfAsSender.MessageSummary
                    ?? throw new Exception("MessageSummary should not be null"));
                messageSummarySha256 = XmlCanonicalizationHelper.GetSha256Hash(memoryStream);
            }
            else
            {
                throw new Exception($"Unknown MessageSummaryVersion {pdfAsSender.MessageSummaryVersion}");
            }

            (string, DateTime?, string?)[] recipients = pdfAsSender
                .Recipients
                .Select(e =>
                {
                    if (e.MessageSummary == null)
                    {
                        return (e.ProfileName, e.DateReceived, null);
                    }

                    byte[] messageSummarySha256;
                    if (pdfAsSender.MessageSummaryVersion == MessageSummaryVersion.V1)
                    {
                        using IEncryptor encryptorV1 = this.encryptorFactoryV1.CreateEncryptor();
                        messageSummarySha256 = SHA256.HashData(
                            encryptorV1.Decrypt(e.MessageSummary));
                    }
                    else if (pdfAsSender.MessageSummaryVersion == MessageSummaryVersion.V2)
                    {
                        using MemoryStream memoryStream = new(e.MessageSummary);
                        messageSummarySha256 = XmlCanonicalizationHelper.GetSha256Hash(memoryStream);
                    }
                    else
                    {
                        throw new Exception($"Unknown MessageSummaryVersion {pdfAsSender.MessageSummaryVersion}");
                    }

                    return (
                        e.ProfileName,
                        e.DateReceived,
                        (string?)EncryptionHelper.GetHexString(messageSummarySha256));
                })
                .ToArray();

            MemoryStream pdf = PdfWriterUtils.CreatePdf(
                newPdf,
                this.pdfOptionsAccessor.Value,
                pdfAsSender.SenderProfileName,
                pdfAsSender.DateSent,
                EncryptionHelper.GetHexString(messageSummarySha256),
                recipients!,
                pdfAsSender.Subject,
                pdfAsSender.Rnu,
                template.Content,
                decryptedBody);

            using MemoryStream signedPdf =
                this.recyclableMemoryStreamManager.GetStream();

            PdfReaderUtils.Sign(
                pdf,
                signedPdf,
                PdfWriterUtils.SignatureField,
                AsSender_SignatureReason,
                AsSender_SignatureLocation,
                this.domainOptionsAccessor.Value.SigningCertificateStore
                    ?? throw new Exception("Missing SigningCertificateStore option"),
                this.domainOptionsAccessor.Value.SigningCertificateStoreLocation
                    ?? throw new Exception("Missing SigningCertificateStoreLocation option"),
                this.domainOptionsAccessor.Value.SigningCertificateThumprint
                    ?? throw new Exception("Missing SigningCertificateThumprint option"));
            signedPdf.Seek(0, SeekOrigin.Begin);

            newPdf.Dispose();

            string fileName = string.Format(
                AsSender_PdfFileName,
                messageId,
                pdfAsSender.DateSent.ToString("ddMMyyyy"));

            // TODO: maybe tell the consumer to download the blob himself
            // instead of proxying the result inefficiently
            return new GetPdfAsSenderVO(
                fileName,
                AsSender_PdfContentType,
                signedPdf.ToArray()); // creates a copy of the recyclable stream
        }
    }
}

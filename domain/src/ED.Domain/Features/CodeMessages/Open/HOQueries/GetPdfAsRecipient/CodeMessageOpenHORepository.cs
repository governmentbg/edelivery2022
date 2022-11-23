using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using static ED.Domain.ICodeMessageOpenHORepository;
using static ED.Domain.IProfilesService;

namespace ED.Domain
{
    partial class CodeMessageOpenHORepository : ICodeMessageOpenHORepository
    {
        private const string AsRecipient_PdfFileName = "Удостоверение за връчване-{0}_{1}.pdf";
        private const string AsRecipient_PdfContentType = "application/pdf";
        private const string AsRecipient_SignatureReason = "Non-Repudiation of Origin";
        private const string AsRecipient_SignatureLocation = "Bulgaria, Sofia";

        public async Task<GetPdfAsRecipientVO> GetPdfAsRecipientAsync(
            Guid accessCode,
            CancellationToken ct)
        {
            ICodeMessageOpenQueryRepository.GetPdfAsRecipientVO pdfAsRecipient =
                await this.codeMessageOpenQueryRepository.GetPdfAsRecipientAsync(
                    accessCode,
                    ct);

            // for legacy messages
            if (pdfAsRecipient.MessagePdfBlobId.HasValue)
            {
                // TODO: maybe tell the consumer to download the blob himself
                // instead of proxying the result inefficiently
                BlobsServiceClient.DownloadBlobToArrayVO blob =
                    await this.blobsServiceClient.DownloadProfileBlobToArrayAsync(
                        pdfAsRecipient.MessagePdfBlobId.Value,
                        pdfAsRecipient.Recipient.ProfileId,
                        ct);

                return new GetPdfAsRecipientVO(
                    blob.FileName,
                    AsRecipient_PdfContentType,
                    blob.Content);
            }

            ICodeMessageOpenQueryRepository.GetMessageAccessKeyVO messageAccessKey =
                await this.codeMessageOpenQueryRepository.GetMessageAccessKeyAsync(
                    pdfAsRecipient.MessageId,
                    pdfAsRecipient.Recipient.ProfileId,
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
                pdfAsRecipient.IV);

            string decryptedBody =
                Encoding.UTF8.GetString(encryptor.Decrypt(pdfAsRecipient.Body));

            string templateContent =
                await this.codeMessageOpenQueryRepository.GetTemplateContentAsync(
                    pdfAsRecipient.TemplateId!.Value,
                    ct);

            using MemoryStream newPdf =
                this.recyclableMemoryStreamManager.GetStream();

            byte[] messageSummarySha256;
            byte[] recipientMessageSummarySha256;
            if (pdfAsRecipient.MessageSummaryVersion == MessageSummaryVersion.V1)
            {
                using IEncryptor encryptorV1 = this.encryptorFactoryV1.CreateEncryptor();
                messageSummarySha256 = SHA256.HashData(
                    encryptorV1.Decrypt(
                        pdfAsRecipient.MessageSummary
                        ?? throw new Exception("MessageSummary should not be null")));

                recipientMessageSummarySha256 = SHA256.HashData(
                    encryptorV1.Decrypt(
                        pdfAsRecipient.Recipient.MessageSummary
                        ?? throw new Exception("Recipient MessageSummary should not be null")));
            }
            else if (pdfAsRecipient.MessageSummaryVersion == MessageSummaryVersion.V2)
            {
                using MemoryStream memoryStream = new(
                    pdfAsRecipient.MessageSummary
                    ?? throw new Exception("MessageSummary should not be null"));
                messageSummarySha256 = XmlCanonicalizationHelper.GetSha256Hash(memoryStream);

                using MemoryStream memoryStream2 = new(
                    pdfAsRecipient.Recipient.MessageSummary
                    ?? throw new Exception("Recipient MessageSummary should not be null"));
                recipientMessageSummarySha256 = XmlCanonicalizationHelper.GetSha256Hash(memoryStream2);
            }
            else
            {
                throw new Exception($"Unknown MessageSummaryVersion {pdfAsRecipient.MessageSummaryVersion}");
            }

            (string Name, DateTime? DateReceived, string? MessageSummaryHash)[] recipients =
            {
                (
                    pdfAsRecipient.Recipient.ProfileName,
                    pdfAsRecipient.Recipient.DateReceived,
                    EncryptionHelper.GetHexString(recipientMessageSummarySha256)
                )
            };

            MemoryStream pdf = PdfWriterUtils.CreatePdf(
                newPdf,
                this.pdfOptionsAccessor.Value,
                pdfAsRecipient.SenderProfileName,
                pdfAsRecipient.DateSent,
                EncryptionHelper.GetHexString(messageSummarySha256),
                recipients,
                pdfAsRecipient.Subject,
                pdfAsRecipient.Rnu,
                templateContent,
                decryptedBody);

            using MemoryStream signedPdf =
                this.recyclableMemoryStreamManager.GetStream();

            PdfReaderUtils.Sign(
                pdf,
                signedPdf,
                PdfWriterUtils.SignatureField,
                AsRecipient_SignatureReason,
                AsRecipient_SignatureLocation,
                this.domainOptionsAccessor.Value.SigningCertificateStore
                    ?? throw new Exception("Missing SigningCertificateStore option"),
                this.domainOptionsAccessor.Value.SigningCertificateStoreLocation
                    ?? throw new Exception("Missing SigningCertificateStoreLocation option"),
                this.domainOptionsAccessor.Value.SigningCertificateThumprint
                    ?? throw new Exception("Missing SigningCertificateThumprint option"));
            signedPdf.Seek(0, SeekOrigin.Begin);

            newPdf.Dispose();

            string fileName = string.Format(
                AsRecipient_PdfFileName,
                pdfAsRecipient.MessageId,
                pdfAsRecipient.DateSent.ToString("ddMMyyyy"));

            // TODO: maybe tell the consumer to download the blob himself
            // instead of proxying the result inefficiently
            return new GetPdfAsRecipientVO(
                fileName,
                AsRecipient_PdfContentType,
                signedPdf.ToArray()); // creates a copy of the recyclable stream
        }
    }
}

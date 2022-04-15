using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using EDelivery.Common.DataContracts;
using EDelivery.Common.Enums;
using MimeTypes;

namespace ED.IntegrationService
{
    public partial class DataContractMapper
    {
        public static List<DcDocument> ToListOfDcDocument(
            ED.DomainServices.IntegrationService.GetSentDocumentsContentResponse resp)
        {
            List<DcDocument> result = new List<DcDocument>();

            foreach(ED.DomainServices.IntegrationService.GetSentDocumentsContentResponse.Types.Blob item in resp.Blobs)
            {
                string ext = Path.GetExtension(item.FileName);

                result.Add(new DcDocument
                {
                    Id = item.BlobId,
                    DocumentName = item.FileName,
                    ContentEncodingCodePage = null,
                    ContentType = string.IsNullOrEmpty(ext)
                        ? null
                        : MimeTypeMap.GetMimeType(ext),
                    Content = item.Content.ToByteArray(),
                    TimeStamp = new DcTimeStamp(
                        item.FileName,
                        item.Timestamp.ToByteArray()),
                    DocumentRegistrationNumber = item.DocumentRegistrationNumber,
                    SignaturesInfo =
                        ToListOfDcSignatureValidationResult(item.Signatures.ToArray()),
                });
            }

            return result;
        }

        public static List<DcSignatureValidationResult> ToListOfDcSignatureValidationResult(
            ED.DomainServices.IntegrationService.GetSentDocumentsContentResponse.Types.Signature[] signatures)
        {
            bool containsTimeStamp = false;
            string timeStampAuthority = null;
            DateTime? timeStampDate = null;
            ED.DomainServices.IntegrationService.GetSentDocumentsContentResponse.Types.Signature tsSig =
                signatures.FirstOrDefault(s => s.IsTimestamp);
            if (tsSig != null)
            {
                containsTimeStamp = true;
                timeStampAuthority = tsSig.Issuer;
                timeStampDate = tsSig.SignDate.ToLocalDateTime();
            }

            var res = new List<DcSignatureValidationResult>();
            foreach (var sig in signatures.Where(s => !s.IsTimestamp))
            {
                using (var cert = new X509Certificate2(sig.SigningCertificate.ToByteArray()))
                {
                    res.Add(
                        new DcSignatureValidationResult
                        {
                            Status = eVerificationResult.Success,
                            RevocationStatus = eRevokationResult.OK,
                            ChainErrors = new List<string>(),
                            ChainCertificates = new List<DcChainCertificate>(),
                            CertificateAlgorithm = cert.SignatureAlgorithm.FriendlyName,
                            ValidTo = cert.NotAfter,
                            ValidFrom = cert.NotBefore,
                            Subject = cert.Subject,
                            SubjectEGN = ExtractEgnFromSubjectOrExtensions(
                                cert.Subject,
                                cert.Extensions),
                            Issuer = cert.Issuer,
                            IsExpired = sig.SignDate.ToLocalDateTime() < cert.NotBefore
                                || sig.SignDate.ToLocalDateTime() > cert.NotAfter,
                            IsTrustedSigner = sig.ValidAtTimeOfSigning,
                            IsSignatureValid = sig.ValidAtTimeOfSigning,
                            IsIntegrityValid = sig.CoversDocument,
                            Format = cert.GetFormat(),
                            ContainsTimeStamp = containsTimeStamp,
                            TimeStampAuthority = timeStampAuthority,
                            TimeStampDate = timeStampDate,
                        });
                }
            }

            if (res.Count == 0)
            {
                res.Add(new DcSignatureValidationResult { Status = eVerificationResult.NoSignatureFound });
            }

            return res;
        }
    }
}

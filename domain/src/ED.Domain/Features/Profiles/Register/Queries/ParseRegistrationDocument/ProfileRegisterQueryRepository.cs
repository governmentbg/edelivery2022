using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using static ED.Domain.IProfileRegisterQueryRepository;

namespace ED.Domain
{
    partial class ProfileRegisterQueryRepository : IProfileRegisterQueryRepository
    {
        private const string Name = "fCompanyName";
        private const string Identifier = "fRegistrationNumber";
        private const string Email = "fEmailAddress";
        private const string Phone = "fPhoneNumber";
        private const string Residence = "fAddress";
        private const string City = "fCity";
        private const string State = "fState";
        private const string Country = "fCountry";

        public async Task<ParseRegistrationDocumentVO> ParseRegistrationDocumentAsync(
            int blobId,
            CancellationToken ct)
        {
            using MemoryStream blobMemoryStream =
                this.recyclableMemoryStreamManager.GetStream();
            BlobsServiceClient.DownloadBlobToStreamVO blob =
                await this.blobsServiceClient.DownloadSystemBlobToStreamAsync(
                    blobId,
                    blobMemoryStream,
                    ct);
            blobMemoryStream.Seek(0, SeekOrigin.Begin);

            try
            {
                IDictionary<string, string?> values =
                    PdfReaderUtils.ExtractFormValues(
                        blobMemoryStream,
                        new[]
                        {
                            Name,
                            Identifier,
                            Email,
                            Phone,
                            Residence,
                            City,
                            State,
                            Country
                        });

                return new ParseRegistrationDocumentVO(
                    true,
                    new ParseRegistrationDocumentVOResult(
                        values[Name]!,
                        values[Identifier]!,
                        values[Phone]!,
                        values[Email]!,
                        values[Residence]!,
                        values[City],
                        values[State],
                        values[Country]));
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                return new ParseRegistrationDocumentVO(false, null);
            }
        }
    }
}

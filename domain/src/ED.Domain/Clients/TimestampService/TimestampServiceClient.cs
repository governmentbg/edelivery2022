using System;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Asn1.Cmp;
using Org.BouncyCastle.Tsp;

namespace ED.Domain
{
    public class TimestampServiceClient
    {
        private readonly HttpClient httpClient;
        private readonly string connectionString;

        public TimestampServiceClient(
            HttpClient httpClient,
            IOptions<DomainOptions> domainOptionsAccessor)
        {
            this.httpClient = httpClient;
            this.connectionString = domainOptionsAccessor.Value.GetConnectionString();
        }

        /// <summary>
        /// Submit a request with the RFC 3161 Time-Stamp Protocol via HTTP/HTTPS
        /// </summary>
        /// <param name="hashAlgorithm">
        /// A case insensitive string describing the hash algorithm used
        /// to compute the hash parameter.
        /// One of: MD5/SHA1/SHA256/SHA512
        /// </param>
        /// <param name="hash">The hash digest</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>The raw ASN.1 DER-encoded Time-Stamp Response message</returns>
        public async Task<byte[]> SubmitAsync(
            int messageId,
            string hashAlgorithm,
            byte[] hash,
            CancellationToken ct)
        {
            long nonce = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            TimeStampRequestGenerator tsReqGenerator = new();

            // we would like the TSA to include its certificate in the response.
            tsReqGenerator.SetCertReq(true);

            string hashAlgorithmOid = this.GetHashAlgorithmOid(hashAlgorithm);
            TimeStampRequest tsReq = tsReqGenerator.Generate(
                hashAlgorithmOid,
                hash,
                new Org.BouncyCastle.Math.BigInteger(nonce.ToString()));

            byte[] tsReqBytes = tsReq.GetEncoded();

            using ByteArrayContent content = new(tsReqBytes);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/timestamp-query");

            HttpResponseMessage? resp = null;
            string? contentString = null;

            try
            {
                resp = await this.httpClient.PostAsync("", content, ct);

                if (!resp.IsSuccessStatusCode)
                {
                    try
                    {
                        // get the first 1000 bytes as string
                        await resp.Content.LoadIntoBufferAsync(1000);
                        contentString = await resp.Content.ReadAsStringAsync(ct);
                    }
#pragma warning disable CA1031 // Do not catch general exception types 
                    catch
#pragma warning restore CA1031
                    {
                        // this a best effort attempt, ignore all errors and continue
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                TimestampException networkTimestampException =
                    new($"TimestampService request failed. StatusCode: {resp?.StatusCode}.\nContent: {contentString}", ex);

                await this.AuditAsync(messageId, TimestampStatus.NetworkError, networkTimestampException.Message, ct);

                throw networkTimestampException;
            }

            if (resp.Content.Headers.ContentType?.MediaType != "application/timestamp-reply")
            {
                TimestampException mediaTimestampException =
                    new($"Received unknown Content-Type. StatusCode: {resp.StatusCode}.\nContent-Type: {resp.Content.Headers.ContentType?.MediaType}");

                await this.AuditAsync(messageId, TimestampStatus.Error, mediaTimestampException.Message, ct);

                throw mediaTimestampException;
            }

            byte[] tsRespBytes = await resp.Content.ReadAsByteArrayAsync(ct);

            TimeStampResponse tsResponse = new(tsRespBytes);

            TimestampException? validateTimestampException = this.ValidateResponse(tsReq, tsResponse);

            if (validateTimestampException != null)
            {
                await this.AuditAsync(messageId, TimestampStatus.Error, validateTimestampException.Message, ct);

                throw validateTimestampException;
            }
            else
            {
                await this.AuditAsync(messageId, TimestampStatus.Success, null, ct);
            }

            return tsRespBytes;
        }

        private TimestampException? ValidateResponse(
            TimeStampRequest request,
            TimeStampResponse response)
        {
            PkiStatus status = (PkiStatus)response.Status;

            if (status == PkiStatus.Granted ||
                status == PkiStatus.GrantedWithMods)
            {
                if (response.TimeStampToken == null)
                {
                    return new TimestampException($"Invalid TS response: missing time stamp token. Response status: {status}");
                }

                if (response.TimeStampToken.TimeStampInfo.Nonce
                    .CompareTo(request.Nonce) != 0)
                {
                    return new TimestampException($"Invalid TS response: nonce mismatch. Response status: {status}");
                }

                if (response.TimeStampToken.TimeStampInfo
                    .GetMessageImprintDigest()
                    .SequenceEqual(request.GetMessageImprintDigest()) == false)
                {
                    return new TimestampException($"Invalid TS response: message imprint mismatch. Response status: {status}");
                }

                if (response.TimeStampToken.TimeStampInfo.MessageImprintAlgOid
                    != request.MessageImprintAlgOid)
                {
                    return new TimestampException("Invalid TS response: message imprint alg oid missmatch");
                }
            }
            else
            {
                string statusString = response.GetStatusString();
                int? failInfo = response.GetFailInfo()?.IntValue;
                string failInfoStr = failInfo == null
                    ? ""
                    : new Org.BouncyCastle.Asn1.Cmp.PkiFailureInfo(failInfo.Value).ToString();
                return new TimestampException($"Invalid TS response. Response status: {status}\nStatus string: {statusString}\nFailInfo: {failInfoStr}");
            }

            return null;
        }

        private async Task AuditAsync(
            int messageId,
            TimestampStatus status,
            string? description,
            CancellationToken ct)
        {
            await using SqlConnection connection = new(this.connectionString);
            await connection.OpenAsync(ct);
            await using SqlCommand cmd = connection.CreateCommand();
            cmd.CommandText =
                $@"INSERT INTO [dbo].[TimeStampRequestsAuditLog] (
                    [MessageId]
                    ,[BlobId]
                    ,[DateSent]
                    ,[Status]
                    ,[Description])
                VALUES (@messageId, NULL, GETDATE(), @status, @desc)";
            cmd.Parameters.AddRange(
                new SqlParameter[] {
                    new SqlParameter("messageId", SqlDbType.Int)
                    {
                        Value = messageId
                    },
                    new SqlParameter("status", SqlDbType.Int)
                    {
                        Value = status
                    },
                    new SqlParameter("desc", SqlDbType.NVarChar, 500)
                    {
                        Value = (object?)description ?? DBNull.Value
                    },
                });

            await cmd.ExecuteNonQueryAsync(ct);
        }

        private string GetHashAlgorithmOid(string hashAlgorithm)
            => hashAlgorithm.ToUpperInvariant() switch
            {
                "MD5" => "1.2.840.113549.2.5",
                "SHA1" => "1.3.14.3.2.26",
                "SHA256" => "2.16.840.1.101.3.4.2.1",
                "SHA512" => "2.16.840.1.101.3.4.2.3",
                _ => throw new TimestampException($"Unsupported hashAlgorithm {hashAlgorithm}"),
            };
    }
}

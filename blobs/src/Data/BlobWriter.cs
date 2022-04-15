using System;
using System.Data;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlTypes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PullStream;
using static ED.Blobs.ProfileKeyService;
using static ED.Keystore.Keystore;

namespace ED.Blobs
{
    public class BlobWriter
    {
        private readonly IEncryptorFactory encryptorFactory;
        private readonly KeystoreClient keystoreClient;
        private readonly MalwareServiceClient malwareServiceClient;
        private readonly TimestampServiceClient timestampServiceClient;
        private readonly MalwareScanResultWriter malwareScanResultWriter;
        private readonly ProfileKeyService profileKeyService;
        private readonly PdfServicesClient pdfServicesClient;
        private readonly ILogger<BlobWriter> logger;
        private readonly string connectionString;
        private readonly string hashAlgorithm;
        private readonly bool malwareScanEnabled;
        private readonly int malwareApiMaxAllowedFileSizeInMb;
        private readonly int extractSignaturesPdfMaxSizeInMb;

        public BlobWriter(
            IEncryptorFactory encryptorFactory,
            KeystoreClient keystoreClient,
            MalwareServiceClient malwareServiceClient,
            TimestampServiceClient timestampServiceClient,
            MalwareScanResultWriter malwareScanResultWriter,
            ProfileKeyService profileKeyService,
            PdfServicesClient pdfServicesClient,
            ILogger<BlobWriter> logger,
            IOptions<BlobsOptions> blobsOptionsAccessor,
            IOptions<DataOptions> dataOptionsAccessor)
        {
            var blobsOptions = blobsOptionsAccessor.Value;
            var dataOptions = dataOptionsAccessor.Value;

            this.encryptorFactory = encryptorFactory;
            this.keystoreClient = keystoreClient;
            this.malwareServiceClient = malwareServiceClient;
            this.timestampServiceClient = timestampServiceClient;
            this.malwareScanResultWriter = malwareScanResultWriter;
            this.profileKeyService = profileKeyService;
            this.pdfServicesClient = pdfServicesClient;
            this.logger = logger;
            this.malwareScanEnabled = blobsOptions.MalwareScanEnabled;
            this.malwareApiMaxAllowedFileSizeInMb = blobsOptions.MalwareApiMaxAllowedFileSizeInMb;
            this.extractSignaturesPdfMaxSizeInMb = blobsOptions.ExtractSignaturesPdfMaxSizeInMb;
            this.connectionString = dataOptions.GetConnectionString()
                ?? throw new Exception($"Missing setting {nameof(DataOptions)}.{nameof(DataOptions.ConnectionString)}");
            this.hashAlgorithm = blobsOptions.HashAlgorithm
                ?? throw new Exception($"Missing setting {nameof(BlobsOptions)}.{nameof(BlobsOptions.HashAlgorithm)}");
        }

        public record BlobInfo(
            int? BlobId,
            long Size,
            string? HashAlgorithm,
            string? Hash,
            MalwareScanStatus MalwareScanStatus,
            BlobSignatureStatus SignatureStatus);

        public enum ProfileBlobAccessKeyType
        {
            Temporary = 0,
            Storage = 1,
            Registration = 2,
            Template = 3,
            PdfStamp = 4,
        }

        public async Task<BlobInfo> UploadProfileBlobFromStreamReaderAsync<T>(
            IAsyncStreamReader<T> streamReader,
            Func<T, ReadOnlyMemory<byte>> chunkReader,
            string fileName,
            long upperSizeLimit,
            bool extractPdfSignatures,
            int profileId,
            int loginId,
            ProfileBlobAccessKeyType type,
            string? documentRegistrationNumber,
            CancellationToken ct)
        {
            // wrap all the chunks in a stream
            var blobStream = SequenceStream.UsingStream()
                .On(streamReader.ReadAllAsync(ct))
                .WithCancellation(ct)
                .Writing(
                    (stream, chunk) =>
                    {
                        stream.Write(chunkReader(chunk).Span);
                    }
                );

            return await this.UploadProfileBlobAsync(
                blobStream,
                fileName,
                upperSizeLimit,
                extractPdfSignatures,
                profileId,
                loginId,
                type,
                documentRegistrationNumber,
                ct);
        }

        public async Task<BlobInfo> UploadProfileBlobAsync(
            Stream fileStream,
            string fileName,
            long upperSizeLimit,
            bool extractPdfSignatures,
            int profileId,
            int loginId,
            ProfileBlobAccessKeyType type,
            string? documentRegistrationNumber,
            CancellationToken ct)
        {
            using IEncryptor encryptor = this.encryptorFactory.CreateEncryptor();

            await using SqlConnection connection = new(this.connectionString);
            await connection.OpenAsync(ct);

            BlobInfo blobInfo = await this.UploadBlobAsync(
                connection,
                encryptor,
                fileStream,
                fileName,
                upperSizeLimit,
                extractPdfSignatures,
                documentRegistrationNumber,
                ct);

            if (blobInfo.MalwareScanStatus != MalwareScanStatus.IsMalicious)
            {
                (int profileKeyId, byte[] encryptedKey) = await this.EncryptKeyAsync(
                    profileId,
                    encryptor.Key,
                    ct);

                await this.InsertProfileBlobAccessKeyAsync(
                    connection,
                    profileId,
                    blobInfo.BlobId!.Value,
                    profileKeyId,
                    loginId,
                    encryptedKey,
                    type,
                    ct);
            }

            return blobInfo;
        }

        private async Task<BlobInfo>
            UploadBlobAsync(
                SqlConnection connection,
                IEncryptor encryptor,
                Stream fileStream,
                string fileName,
                long upperSizeLimit,
                bool extractPdfSignatures,
                string? documentRegistrationNumber,
                CancellationToken ct)
        {
            bool performMalwareScan =
                    this.malwareScanEnabled
                    && (this.malwareApiMaxAllowedFileSizeInMb * 1024L * 1024L) > upperSizeLimit;
            bool performSignatureExtraction =
                Path.GetExtension(fileName).Equals(".pdf", StringComparison.InvariantCultureIgnoreCase)
                && extractPdfSignatures
                && (this.extractSignaturesPdfMaxSizeInMb * 1024L * 1024L) > upperSizeLimit;

            (int blobId, string path) =
                await this.InsertBlobAsync(
                    connection,
                    fileName,
                    encryptor.IV,
                    documentRegistrationNumber,
                    ct);

            long size;
            byte[] hash;
            string? malwareScanId;
            SignatureInfo[] signatures;

            // use a separate block for the transaction
            {
                await using SqlTransaction transaction =
                    (SqlTransaction)await connection.BeginTransactionAsync(ct);

                byte[] transactionContext =
                    await this.GetTransactionContextAsync(
                        connection,
                        transaction,
                        ct);

                using HashAlgorithm hashTransform = HashAlgorithm.Create(this.hashAlgorithm)
                    ?? throw new Exception($"'{this.hashAlgorithm}' is not a valid hash algorithm.");

                // use a separate block to close the streams after
                // the file is completely read
                {
                    using Stream sqlFileStream = new SqlFileStream(
                        path,
                        transactionContext,
                        FileAccess.Write,
                        FileOptions.SequentialScan,
                        allocationSize: 0);
                    using CryptoStream encryptionStream =
                        new(sqlFileStream, encryptor.CreateEncryptor(), CryptoStreamMode.Write);
                    using CryptoStream hashStream =
                        new(encryptionStream, hashTransform, CryptoStreamMode.Write);

                    object?[] results;
                    (size, results) =
                        await this.CopyToMultipleDestinations(
                            fileStream,
                            new (Stream, Func<CancellationToken, Task<object?>>)[]
                            {
                                (hashStream, (_) => Task.FromResult<object?>(null)),
                                performMalwareScan
                                    ? this.MalwareScan(fileName, blobId)
                                    : (Stream.Null, (_) => Task.FromResult<object?>(null)),
                                performSignatureExtraction
                                    ? this.SignatureExtraction(fileName, blobId)
                                    : (Stream.Null, (_) => Task.FromResult<object?>(Array.Empty<SignatureInfo>())),
                            },
                            ct);

                    malwareScanId = (string?)results[1];
                    signatures = (SignatureInfo[])results[2]!;
                }

                await transaction.CommitAsync(ct);

                hash = hashTransform.Hash ?? throw new Exception("Missing hash.");
            }

            int? malwareScanResultId;
            MalwareScanStatus malwareScanStatus;
            if (performMalwareScan)
            {
                if (malwareScanId == null)
                {
                    malwareScanStatus = MalwareScanStatus.NotSure;
                    malwareScanResultId = null;
                }
                else
                {
                    (malwareScanStatus, malwareScanResultId) =
                        await this.malwareScanResultWriter.InsertResultAsync(
                            connection,
                            malwareScanId,
                            ct);

                    if (malwareScanStatus == MalwareScanStatus.IsMalicious)
                    {
                        return new BlobInfo(
                            null,
                            size,
                            null,
                            null,
                            MalwareScanStatus.IsMalicious,
                            BlobSignatureStatus.None);
                    }
                }
            }
            else
            {
                malwareScanStatus = MalwareScanStatus.None;
                malwareScanResultId = null;
            }

            BlobSignatureStatus signatureStatus =
                await this.InsertPdfSignaturesAsync(
                    connection,
                    blobId,
                    signatures,
                    ct);

            byte[] timestamp = await this.timestampServiceClient.SubmitAsync(
                blobId,
                this.hashAlgorithm,
                hash,
                ct);

            await this.UpdateBlobAsync(
                connection,
                blobId,
                hash,
                this.hashAlgorithm,
                timestamp,
                size,
                malwareScanResultId,
                ct);

            return new BlobInfo(
                blobId,
                size,
                this.hashAlgorithm,
                BlobUtils.GetHexString(hash),
                malwareScanStatus,
                signatureStatus);
        }

        /// <summary>
        /// Copy a single stream to multiple destinations in parallel with
        /// the execution of a stream consuming action
        /// </summary>
        private async Task<(long size, object?[] results)> CopyToMultipleDestinations(
            Stream source,
            (Stream stream,
            Func<CancellationToken, Task<object?>> consumeAction)[] destinations,
            CancellationToken ct)
        {
            var streams = new Stream[destinations.Length];
            var consumeActions = new Func<CancellationToken, Task<object?>>[destinations.Length];
            for (int i = 0; i < destinations.Length; i++)
            {
                var (stream, consumeAction) = destinations[i];
                streams[i] = stream;
                consumeActions[i] = consumeAction;
            }

            Task<long> copyTask = source.CopyToMultipleAsync(
                streams,
                BlobConstants.DefaultCopyBufferSize,
                ct);

            Task<object?[]> consumeTask = Task.WhenAll(
                consumeActions
                    .Select(a => a(ct))
                    .ToArray());

            long size = await copyTask;

            foreach (Stream stream in streams)
            {
                stream.Close();
            }

            object?[] results = await consumeTask;

            return (size, results);
        }

        private (Stream, Func<CancellationToken, Task<object?>>) MalwareScan(
            string fileName,
            int blobId)
        {
            Pipe pipe =
                new Pipe(
                    new PipeOptions(
                        minimumSegmentSize: BlobConstants.PipeMinimumSegmentSize,
                        pauseWriterThreshold: BlobConstants.PipePauseWriterThreshold,
                        resumeWriterThreshold: BlobConstants.PipeResumeWriterThreshold
                    ));

            async Task<object?> ScanAsync(CancellationToken ct)
            {
                string? malwareScanId;
                try
                {
                    malwareScanId =
                        await this.malwareServiceClient.SubmitAsync(
                            pipe.Reader.AsStream(),
                            fileName,
                            null,
                            ct);
                }
                catch (MalwareServiceClientException ex)
                {
                    this.logger.LogError(ex, $"MalwareScan Submit failed for fileName {fileName}, blobId {blobId}");
                    malwareScanId = null;
                }

                return malwareScanId;
            }

            return (
                pipe.Writer.AsStream(),
                ScanAsync);
        }

        private (Stream, Func<CancellationToken, Task<object?>>) SignatureExtraction(
            string fileName,
            int blobId)
        {
            Pipe pipe =
                new Pipe(
                    new PipeOptions(
                        minimumSegmentSize: BlobConstants.PipeMinimumSegmentSize,
                        pauseWriterThreshold: BlobConstants.PipePauseWriterThreshold,
                        resumeWriterThreshold: BlobConstants.PipeResumeWriterThreshold
                    ));

            async Task<object?> ExtractAsync(CancellationToken ct)
            {
                try
                {
                    return await this.pdfServicesClient.ExtractSignaturesAsync(
                        pipe.Reader.AsStream(),
                        fileName,
                        blobId,
                        ct);
                }
                catch (PdfServicesClientException ex)
                {
                    this.logger.LogError(
                        ex,
                        $"Failed extracting signatures for fileName {fileName}, blobId {blobId}");
                    return Array.Empty<SignatureInfo>();
                }
            }

            return (
                pipe.Writer.AsStream(),
                ExtractAsync);
        }

        private async Task<(int blobId, string path)>
            InsertBlobAsync(
                SqlConnection connection,
                string fileName,
                byte[] IV,
                string? documentRegistrationNumber,
                CancellationToken ct)
        {
            await using SqlCommand cmd = connection.CreateCommand();

            cmd.CommandText =
                $@"INSERT INTO [dbo].[Blobs] (
                    [BlobId],
                    [FileName],
                    [EncryptedContent],
                    [IV],
                    [DocumentRegistrationNumber],
                    [CreateDate],
                    [ModifyDate])
                OUTPUT
                    inserted.[BlobId],
                    inserted.[EncryptedContent].PathName()
                VALUES (
                    NEXT VALUE FOR [dbo].[BlobsIdSequence],
                    @fileName,
                    CAST('' AS VARBINARY(MAX)),
                    @IV,
                    @documentRegistrationNumber,
                    GETDATE(),
                    GETDATE())";
            cmd.Parameters.AddRange(
                new SqlParameter[]
                {
                    new SqlParameter("fileName", SqlDbType.NVarChar, 500)
                    {
                        Value = fileName
                    },
                    new SqlParameter("IV", SqlDbType.Binary, 16)
                    {
                        Value = IV
                    },
                    new SqlParameter("documentRegistrationNumber", SqlDbType.NVarChar, 255)
                    {
                        Value = (object?)documentRegistrationNumber ?? DBNull.Value
                    },
                });

            await using SqlDataReader reader = await cmd.ExecuteReaderAsync(ct);
            if (!(await reader.ReadAsync(ct)) || (await reader.IsDBNullAsync(0, ct)))
            {
                throw new Exception("The inserted ID should have been returned.");
            }

            int blobId = reader.GetInt32(0);
            string path = reader.GetString(1);

            return (blobId, path);
        }

        private async Task<byte[]> GetTransactionContextAsync(
            SqlConnection connection,
            SqlTransaction transaction,
            CancellationToken ct)
        {
            await using SqlCommand cmd = connection.CreateCommand();

            cmd.CommandText =
                $@"SELECT GET_FILESTREAM_TRANSACTION_CONTEXT()";
            cmd.Transaction = transaction;

            await using SqlDataReader reader = await cmd.ExecuteReaderAsync(ct);
            if (!(await reader.ReadAsync(ct)) || (await reader.IsDBNullAsync(0, ct)))
            {
                throw new Exception("The transaction context could not be retrieved");
            }

            byte[] transactionContext = reader.GetSqlBytes(0).Buffer!;

            return transactionContext;
        }

        private async Task UpdateBlobAsync(
            SqlConnection connection,
            int blobId,
            byte[] hash,
            string hashAlgorithm,
            byte[] timestamp,
            long size,
            int? malwareScanResultId,
            CancellationToken ct)
        {
            await using SqlCommand cmd = connection.CreateCommand();
            cmd.CommandText =
                $@"UPDATE [dbo].[Blobs]
                SET
                    [Hash] = @hash,
                    [HashAlgorithm] = @hashAlgorithm,
                    [Timestamp] = @timestamp,
                    [Size] = @size,
                    [MalwareScanResultId] = @malwareScanResultId,
                    [ModifyDate] = GETDATE()
                WHERE BlobId = @blobId";
            cmd.Parameters.AddRange(
                new[]
                {
                    new SqlParameter("blobId", SqlDbType.Int)
                    {
                        Value = blobId
                    },
                    new SqlParameter("hash", SqlDbType.NVarChar, 64)
                    {
                        Value = BlobUtils.GetHexString(hash)
                    },
                    new SqlParameter("hashAlgorithm", SqlDbType.NVarChar, 10)
                    {
                        Value = hashAlgorithm
                    },
                    new SqlParameter("timestamp", SqlDbType.VarBinary, 8000)
                    {
                        Value = timestamp
                    },
                    new SqlParameter("size", SqlDbType.BigInt)
                    {
                        Value = size
                    },
                    new SqlParameter("malwareScanResultId", SqlDbType.Int)
                    {
                        Value = (object?)malwareScanResultId ?? DBNull.Value
                    }
                });

            await cmd.ExecuteNonQueryAsync(ct);
        }

        private async Task<(int profileKeyId, byte[] encryptedKey)> EncryptKeyAsync(
            int profileId,
            byte[] encryptorKey,
            CancellationToken ct)
        {
            ProfileKeyVO profileKey =
                await this.profileKeyService.GetOrCreateProfileKeyAsync(profileId, ct);

            var encryptedKeyResp = await this.keystoreClient.EncryptWithRsaKeyAsync(
                request: new Keystore.EncryptWithRsaKeyRequest
                {
                    Key = new Keystore.RsaKey
                    {
                        Provider = profileKey.Provider,
                        KeyName = profileKey.KeyName,
                        OaepPadding = profileKey.OaepPadding,
                    },
                    Plaintext = ByteString.CopyFrom(encryptorKey),
                },
                cancellationToken: ct);

            return (
                profileKey.ProfileKeyId,
                encryptedKey: encryptedKeyResp.EncryptedData.ToByteArray()
            );
        }

        private async Task InsertProfileBlobAccessKeyAsync(
            SqlConnection connection,
            int profileId,
            int blobId,
            int profileKeyId,
            int loginId,
            byte[] encryptedKey,
            ProfileBlobAccessKeyType type,
            CancellationToken ct)
        {
            await using SqlCommand cmd = connection.CreateCommand();
            cmd.CommandText =
                $@"INSERT INTO [dbo].[ProfileBlobAccessKeys] (
                    [ProfileId],
                    [BlobId],
                    [ProfileKeyId],
                    [CreatedByLoginId],
                    [EncryptedKey],
                    [Type])
                VALUES (
                    @profileId,
                    @blobId,
                    @profileKeyId,
                    @loginId,
                    @encryptedKey,
                    @type)";
            cmd.Parameters.AddRange(
                new[]
                {
                    new SqlParameter("profileId", SqlDbType.Int)
                    {
                        Value = profileId
                    },
                    new SqlParameter("blobId", SqlDbType.Int)
                    {
                        Value = blobId
                    },
                    new SqlParameter("profileKeyId", SqlDbType.Int)
                    {
                        Value = profileKeyId
                    },
                    new SqlParameter("loginId", SqlDbType.Int)
                    {
                        Value = loginId
                    },
                    new SqlParameter("encryptedKey", SqlDbType.Binary, 256)
                    {
                        Value = encryptedKey
                    },
                    new SqlParameter("type", SqlDbType.Int)
                    {
                        Value = type
                    },
                });

            await cmd.ExecuteNonQueryAsync(ct);
        }

        public async Task<BlobSignatureStatus> InsertPdfSignaturesAsync(
            SqlConnection connection,
            int blobId,
            SignatureInfo[] signatures,
            CancellationToken ct)
        {
            if (signatures.Length == 0)
            {
                return BlobSignatureStatus.None;
            }

            await using SqlCommand cmd = connection.CreateCommand();
            cmd.CommandText =
                $@"INSERT INTO [dbo].[BlobSignatures] (
                    [BlobSignatureId],
                    [BlobId],
                    [X509Certificate2DER],
                    [CoversDocument],
                    [CoversPriorRevision],
                    [SignDate],
                    [IsTimestamp],
                    [ValidAtTimeOfSigning],
                    [Issuer],
                    [Subject],
                    [SerialNumber],
                    [Version],
                    [ValidFrom],
                    [ValidTo]
                )
                VALUES ";
            cmd.Parameters.Add(
                new SqlParameter($"blobId", SqlDbType.Int)
                {
                    Value = blobId
                });

            BlobSignatureStatus status = BlobSignatureStatus.Valid;
            for (int i = 0; i < signatures.Length; i++)
            {
                (byte[] signingCertificate,
                bool coversDocument,
                bool coversPriorRevision,
                bool isTimestamp,
                DateTime signDate,
                bool validAtTimeOfSigning,
                string issuer,
                string subject,
                string serialNumber,
                int version,
                DateTime validFrom,
                DateTime validTo) = signatures[i];

                // return the first non-valid status
                if (status == BlobSignatureStatus.Valid)
                {
                    if (!coversDocument)
                    {
                        status = BlobSignatureStatus.InvalidIntegrity;
                    }
                    else if (signDate < validFrom ||
                        signDate > validTo)
                    {
                        status = BlobSignatureStatus.CertificateExpiredAtTimeOfSigning;
                    }
                    else if (!validAtTimeOfSigning)
                    {
                        status = BlobSignatureStatus.InvalidCertificate;
                    }
                }

                if (i > 0)
                {
                    cmd.CommandText += ", ";
                }

#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                cmd.CommandText += $@"(
                    NEXT VALUE FOR [dbo].[BlobSignaturesIdSequence],
                    @blobId,
                    @x509Certificate2DER{i},
                    @coversDocument{i},
                    @coversPriorRevision{i},
                    @signDate{i},
                    @isTimestamp{i},
                    @validAtTimeOfSigning{i},
                    @issuer{i},
                    @subject{i},
                    @serialNumber{i},
                    @version{i},
                    @validFrom{i},
                    @validTo{i}
                )";
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities

                cmd.Parameters.AddRange(
                    new SqlParameter[]
                    {
                        new SqlParameter($"x509Certificate2DER{i}", SqlDbType.VarBinary, -1 /* max */)
                        {
                            Value = signingCertificate
                        },
                        new SqlParameter($"coversDocument{i}", SqlDbType.Bit)
                        {
                            Value = coversDocument
                        },
                        new SqlParameter($"coversPriorRevision{i}", SqlDbType.Bit)
                        {
                            Value = coversPriorRevision
                        },
                        new SqlParameter($"signDate{i}", SqlDbType.DateTime2)
                        {
                            Value = signDate
                        },
                        new SqlParameter($"isTimestamp{i}", SqlDbType.Bit)
                        {
                            Value = isTimestamp
                        },

                        new SqlParameter($"validAtTimeOfSigning{i}", SqlDbType.Bit)
                        {
                            Value = validAtTimeOfSigning
                        },
                        new SqlParameter($"issuer{i}", SqlDbType.NVarChar, 1000)
                        {
                            Value = issuer.Truncate(1000)
                        },
                        new SqlParameter($"subject{i}", SqlDbType.NVarChar, 1000)
                        {
                            Value = subject.Truncate(1000)
                        },
                        new SqlParameter($"serialNumber{i}", SqlDbType.NVarChar, 100)
                        {
                            Value = serialNumber
                        },
                        new SqlParameter($"version{i}", SqlDbType.Int)
                        {
                            Value = version
                        },
                        new SqlParameter($"validFrom{i}", SqlDbType.DateTime2)
                        {
                            Value = validFrom
                        },
                        new SqlParameter($"validTo{i}", SqlDbType.DateTime2)
                        {
                            Value = validTo
                        },
                    });
            }

            await cmd.ExecuteNonQueryAsync(ct);

            return status;
        }
    }
}

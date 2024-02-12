using System;
using System.Buffers;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlTypes;
using Microsoft.Extensions.Options;

namespace ED.Blobs
{
    public class BlobReader
    {
        // use half the maxArrayLength of ArrayPool<byte>.Shared
        // as we use it to create a chunk buffer twice the size of the chunk
        private const int ChunkSize = 512 * 1024; // 512KB

        private readonly IEncryptorFactory encryptorFactory;
        private readonly ED.Keystore.Keystore.KeystoreClient keystoreClient;
        private readonly string connectionString;

        public BlobReader(
            IEncryptorFactory encryptorFactory,
            ED.Keystore.Keystore.KeystoreClient keystoreClient,
            IOptions<DataOptions> dataOptionsAccessor)
        {
            this.encryptorFactory = encryptorFactory;
            this.keystoreClient = keystoreClient;
            this.connectionString = dataOptionsAccessor.Value.GetConnectionString()
                ?? throw new Exception($"Missing setting {nameof(DataOptions)}.{nameof(DataOptions.ConnectionString)}");
        }

        public record BlobInfoVO(string Filename, long Size, byte[] Version);
        public async Task<BlobInfoVO?> GetBlobInfoAsync(
            int blobId,
            CancellationToken ct)
        {
            await using SqlConnection connection = new(this.connectionString);
            await connection.OpenAsync(ct);
            await using SqlCommand cmd = connection.CreateCommand();
            cmd.CommandText =
                $@"SELECT [FileName], [Size], [Version]
                FROM [dbo].[Blobs]
                WHERE [BlobId] = @blobId";
            cmd.Parameters.AddWithValue("@blobId", blobId);

            await using SqlDataReader reader = await cmd.ExecuteReaderAsync(ct);

            if (!reader.HasRows)
            {
                return null;
            }

            await reader.ReadAsync(ct);

            string filename = reader.GetString(reader.GetOrdinal("FileName"));
            long size = reader.GetInt64(reader.GetOrdinal("Size"));
            byte[] version = new byte[8];
            reader.GetBytes(reader.GetOrdinal("Version"), 0, version, 0, 8);

            return new BlobInfoVO(filename, size, version);
        }

        public async Task<bool> IsProfileQuotaEnoughAsync(
            int profileId,
            long upperSizeLimit,
            CancellationToken ct)
        {
            await using SqlConnection connection = new(this.connectionString);
            await connection.OpenAsync(ct);

            long profileStorageQuota = await this.GetProfileStorageQuotaAsync(
                connection,
                profileId,
                ct);

            long usedStorageSpace = await this.GetUsedProfileStorageSpaceAsync(
                connection,
                profileId,
                ct);

            return profileStorageQuota > usedStorageSpace + upperSizeLimit;
        }

        public async Task CopyProfileBlobContentToStreamAsync(
            int profileId,
            int blobId,
            Stream destination,
            int bufferSize,
            CancellationToken ct)
        {
            await using SqlConnection connection = new(this.connectionString);
            await connection.OpenAsync(ct);
            await using SqlTransaction transaction =
                (SqlTransaction)await connection.BeginTransactionAsync(ct);

            var decryptedKey =
                await this.GetDecryptedProfileBlobAccessKeyAsync(
                    connection,
                    transaction,
                    profileId,
                    blobId,
                    ct);

            await this.CopyBlobContentToStreamInternalAsync(
                connection,
                transaction,
                decryptedKey,
                blobId,
                destination,
                bufferSize,
                null,
                null,
                ct);
        }

        public async Task CopyProfileBlobContentToStreamAsync(
            int profileId,
            int blobId,
            Stream destination,
            int bufferSize,
            long offset,
            long length,
            CancellationToken ct)
        {
            await using SqlConnection connection = new(this.connectionString);
            await connection.OpenAsync(ct);
            await using SqlTransaction transaction =
                (SqlTransaction)await connection.BeginTransactionAsync(ct);

            var decryptedKey =
                await this.GetDecryptedProfileBlobAccessKeyAsync(
                    connection,
                    transaction,
                    profileId,
                    blobId,
                    ct);

            await this.CopyBlobContentToStreamInternalAsync(
                connection,
                transaction,
                decryptedKey,
                blobId,
                destination,
                bufferSize,
                offset,
                length,
                ct);
        }

        public async Task CopyMessageBlobContentToStreamAsync(
            int profileId,
            int messageId,
            int blobId,
            Stream destination,
            int bufferSize,
            CancellationToken ct)
        {
            await using SqlConnection connection = new(this.connectionString);
            await connection.OpenAsync(ct);
            await using SqlTransaction transaction =
                (SqlTransaction)await connection.BeginTransactionAsync(ct);

            var decryptedKey =
                await this.GetDecryptedMessageBlobAccessKeyAsync(
                    connection,
                    transaction,
                    profileId,
                    messageId,
                    blobId,
                    ct);

            await this.CopyBlobContentToStreamInternalAsync(
                connection,
                transaction,
                decryptedKey,
                blobId,
                destination,
                bufferSize,
                null,
                null,
                ct);
        }

        public async Task CopyMessageBlobContentToStreamAsync(
            int profileId,
            int messageId,
            int blobId,
            Stream destination,
            int bufferSize,
            long offset,
            long length,
            CancellationToken ct)
        {
            await using SqlConnection connection = new(this.connectionString);
            await connection.OpenAsync(ct);
            await using SqlTransaction transaction =
                (SqlTransaction)await connection.BeginTransactionAsync(ct);

            var decryptedKey =
                await this.GetDecryptedMessageBlobAccessKeyAsync(
                    connection,
                    transaction,
                    profileId,
                    messageId,
                    blobId,
                    ct);

            await this.CopyBlobContentToStreamInternalAsync(
                connection,
                transaction,
                decryptedKey,
                blobId,
                destination,
                bufferSize,
                offset,
                length,
                ct);
        }

        public async Task CopyMessageBlobContentToStreamWriterAsync<T>(
            int profileId,
            int messageId,
            int blobId,
            IServerStreamWriter<T> streamWriter,
            Func<ReadOnlyMemory<byte>, T> chunkFactory,
            CancellationToken ct)
        {
            await using SqlConnection connection = new(this.connectionString);
            await connection.OpenAsync(ct);
            await using SqlTransaction transaction =
                (SqlTransaction)await connection.BeginTransactionAsync(ct);

            var decryptedKey =
                await this.GetDecryptedMessageBlobAccessKeyAsync(
                    connection,
                    transaction,
                    profileId,
                    messageId,
                    blobId,
                    ct);

            (string path, byte[] transactionContext, byte[] IV) =
                await this.GetBlobAsync(connection, transaction, blobId, ct);

            var encryptor = this.encryptorFactory.CreateEncryptor(decryptedKey, IV);

            using (SqlFileStream sqlFileStream =
                new(
                    path,
                    transactionContext,
                    FileAccess.Read,
                    FileOptions.SequentialScan,
                    allocationSize: 0))
            using (CryptoStream decryptionStream =
                new(
                    sqlFileStream,
                    encryptor.CreateDecryptor(),
                    CryptoStreamMode.Read))
            {
                // make the buffer twice the size of the chunk
                // as we are making continuous chunk sized reads(which could read less)
                // until we have enough for a whole chunk and that could be
                // at most ChunkSize * 2 - 1 bytes, which will fit in our buffer
                byte[] buffer = ArrayPool<byte>.Shared.Rent(ChunkSize * 2);
                Memory<byte> memory = new(buffer);
                try
                {
                    int read;
                    int totalRead = 0;

                    while (true)
                    {
                        // IServerStreamWriter.WriteAsync does not have a cancellation token
                        // as its supposed to be able for the consumer of the GRPC service
                        // to be able to gracefully cancel the request but that makes
                        // no sense in the context of blob download, so we just check
                        // and throw an exception if the request was cancelled
                        ct.ThrowIfCancellationRequested();

                        // read until we have a whole chunk
                        do
                        {
                            read = await decryptionStream.ReadAsync(
                                memory.Slice(totalRead, ChunkSize),
                                ct);
                            totalRead += read;
                        }
                        while (read > 0 && totalRead < ChunkSize);

                        if (totalRead > 0)
                        {
                            await streamWriter.WriteAsync(
                                chunkFactory(
                                    memory.Slice(
                                        0,
                                        // account for the last(possibly incomplete) chunk
                                        Math.Min(totalRead, ChunkSize))));
                        }

                        if (read == 0)
                        {
                            break;
                        }

                        // copy the leftovers to the beginning of the buffer
                        Buffer.BlockCopy(buffer, ChunkSize, buffer, 0, totalRead - ChunkSize);
                        totalRead -= ChunkSize;
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }

            await transaction.CommitAsync(ct);
        }

        public async Task CopyProfileBlobContentToStreamWriterAsync<T>(
            int blobId,
            IServerStreamWriter<T> streamWriter,
            Func<ReadOnlyMemory<byte>, T> chunkFactory,
            int profileId,
            CancellationToken ct)
        {
            await using SqlConnection connection = new(this.connectionString);
            await connection.OpenAsync(ct);
            await using SqlTransaction transaction =
                (SqlTransaction)await connection.BeginTransactionAsync(ct);

            var decryptedKey =
                await this.GetDecryptedProfileBlobAccessKeyAsync(
                    connection,
                    transaction,
                    profileId,
                    blobId,
                    ct);

            (string path, byte[] transactionContext, byte[] IV) =
                await this.GetBlobAsync(connection, transaction, blobId, ct);

            var encryptor = this.encryptorFactory.CreateEncryptor(decryptedKey, IV);

            using (SqlFileStream sqlFileStream =
                new(
                    path,
                    transactionContext,
                    FileAccess.Read,
                    FileOptions.SequentialScan,
                    allocationSize: 0))
            using (CryptoStream decryptionStream =
                new(
                    sqlFileStream,
                    encryptor.CreateDecryptor(),
                    CryptoStreamMode.Read))
            {
                // make the buffer twice the size of the chunk
                // as we are making continuous chunk sized reads(which could read less)
                // until we have enough for a whole chunk and that could be
                // at most ChunkSize * 2 - 1 bytes, which will fit in our buffer
                byte[] buffer = ArrayPool<byte>.Shared.Rent(ChunkSize * 2);
                Memory<byte> memory = new(buffer);
                try
                {
                    int read;
                    int totalRead = 0;

                    while (true)
                    {
                        // IServerStreamWriter.WriteAsync does not have a cancellation token
                        // as its supposed to be able for the consumer of the GRPC service
                        // to be able to gracefully cancel the request but that makes
                        // no sense in the context of blob download, so we just check
                        // and throw an exception if the request was cancelled
                        ct.ThrowIfCancellationRequested();

                        // read until we have a whole chunk
                        do
                        {
                            read = await decryptionStream.ReadAsync(
                                memory.Slice(totalRead, ChunkSize),
                                ct);
                            totalRead += read;
                        }
                        while (read > 0 && totalRead < ChunkSize);

                        if (totalRead > 0)
                        {
                            await streamWriter.WriteAsync(
                                chunkFactory(
                                    memory.Slice(
                                        0,
                                        // account for the last(possibly incomplete) chunk
                                        Math.Min(totalRead, ChunkSize))));
                        }

                        if (read == 0)
                        {
                            break;
                        }

                        // copy the leftovers to the beginning of the buffer
                        Buffer.BlockCopy(buffer, ChunkSize, buffer, 0, totalRead - ChunkSize);
                        totalRead -= ChunkSize;
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }

            await transaction.CommitAsync(ct);
        }

        public record MessageTranslationRequestInfoVO(int ProfileId, string? FileName);
        public async Task<MessageTranslationRequestInfoVO?> MessageTranslationRequestInfoAsync(
            int messageTranslationId,
            long requestId,
            string targetLanguage,
            CancellationToken ct)
        {
            await using SqlConnection connection = new(this.connectionString);
            await connection.OpenAsync(ct);

            await using SqlCommand cmd = connection.CreateCommand();
            cmd.CommandText =
                @"SELECT
                    mt.[ProfileId],
                    b.[FileName]
                FROM [MessageTranslationRequests] mtr
                JOIN [MessageTranslations] mt
                    ON mtr.[MessageTranslationId] = mt.[MessageTranslationId]
                LEFT JOIN [dbo].[Blobs] b
                    ON mtr.[SourceBlobId] = b.[BlobId]
                WHERE mtr.[RequestId] = @requestId
                    AND mtr.[TargetBlobId] IS NULL
                    AND mt.[MessageTranslationId] = @messageTranslationId
                    AND mt.[TargetLanguage] = @targetLanguage
                    AND mt.[ArchiveDate] IS NULL";
            cmd.Parameters.AddWithValue("@requestId", requestId);
            cmd.Parameters.AddWithValue("@messageTranslationId", messageTranslationId);
            cmd.Parameters.AddWithValue("@targetLanguage", targetLanguage);

            await using SqlDataReader reader = await cmd.ExecuteReaderAsync(ct);

            if (!reader.HasRows)
            {
                return null;
            }

            await reader.ReadAsync(ct);

            int profileId = reader.GetInt32(reader.GetOrdinal("ProfileId"));
            string? filename = reader.IsDBNull(reader.GetOrdinal("FileName"))
                ? null
                : reader.GetString(reader.GetOrdinal("FileName"));

            return new MessageTranslationRequestInfoVO(profileId, filename);
        }

        private async Task<byte[]> GetDecryptedProfileBlobAccessKeyAsync(
            SqlConnection connection,
            SqlTransaction transaction,
            int profileId,
            int blobId,
            CancellationToken ct)
        {
            var (encryptedKey, provider, keyName, oaepPadding) =
                await this.GetEncryptedProfileBlobAccessKeyAsync(
                    connection,
                    transaction,
                    profileId,
                    blobId,
                    ct);

            var decryptedKeyResp = this.keystoreClient.DecryptWithRsaKey(
                request: new Keystore.DecryptWithRsaKeyRequest
                {
                    Key = new Keystore.RsaKey
                    {
                        Provider = provider,
                        KeyName = keyName,
                        OaepPadding = oaepPadding
                    },
                    EncryptedData = ByteString.CopyFrom(encryptedKey)
                },
                cancellationToken: ct);

            return decryptedKeyResp.Plaintext.ToByteArray();
        }

        private async Task<byte[]> GetDecryptedMessageBlobAccessKeyAsync(
            SqlConnection connection,
            SqlTransaction transaction,
            int profileId,
            int messageId,
            int blobId,
            CancellationToken ct)
        {
            var (encryptedKey, provider, keyName, oaepPadding) =
                await this.GetEncryptedMessageBlobAccessKeyAsync(
                    connection,
                    transaction,
                    profileId,
                    messageId,
                    blobId,
                    ct);

            var decryptedKeyResp = this.keystoreClient.DecryptWithRsaKey(
                request: new Keystore.DecryptWithRsaKeyRequest
                {
                    Key = new Keystore.RsaKey
                    {
                        Provider = provider,
                        KeyName = keyName,
                        OaepPadding = oaepPadding
                    },
                    EncryptedData = ByteString.CopyFrom(encryptedKey)
                },
                cancellationToken: ct);

            return decryptedKeyResp.Plaintext.ToByteArray();
        }

        private async Task<(byte[] encryptedKey, string provider, string keyName, string oaepPadding)>
            GetEncryptedProfileBlobAccessKeyAsync(
                SqlConnection connection,
                SqlTransaction transaction,
                int profileId,
                int blobId,
                CancellationToken ct)
        {
            await using SqlCommand cmd = connection.CreateCommand();
            cmd.CommandText =
                $@"SELECT
                    pbak.[EncryptedKey],
                    pk.[Provider],
                    pk.[KeyName],
                    pk.[OaepPadding]
                FROM [dbo].[ProfileBlobAccessKeys] pbak
                JOIN [dbo].[ProfileKeys] pk ON pbak.[ProfileKeyId] = pk.[ProfileKeyId]
                WHERE pbak.[ProfileId] = @profileId
                    AND pbak.[BlobId] = @blobId";
            cmd.Parameters.AddWithValue("@profileId", profileId);
            cmd.Parameters.AddWithValue("@blobId", blobId);
            cmd.Transaction = transaction;

            await using SqlDataReader reader = await cmd.ExecuteReaderAsync(ct);
            if (!(await reader.ReadAsync(ct)) ||
                (await reader.IsDBNullAsync(0, ct)))
            {
                throw new Exception("A ProfileBlobAccessKey with the provided (profileId, blobId) does not exist.");
            }

            byte[] encryptedKey = reader.GetSqlBytes(0).Buffer
                ?? throw new Exception("EncryptedKey should not be null.");
            string provider = reader.GetString(1);
            string keyName = reader.GetString(2);
            string oaepPadding = reader.GetString(3);

            return (
                encryptedKey,
                provider,
                keyName,
                oaepPadding
            );
        }

        private async Task<(byte[] encryptedKey, string provider, string keyName, string oaepPadding)>
            GetEncryptedMessageBlobAccessKeyAsync(
                SqlConnection connection,
                SqlTransaction transaction,
                int profileId,
                int messageId,
                int blobId,
                CancellationToken ct)
        {
            await using SqlCommand cmd = connection.CreateCommand();

            // there may be more than one record in MessageBlobs with
            // the specified (messageId, blobId) but they should all have the same
            // EncryptedKey and ProfileKeyId so we take the first one
            cmd.CommandText =
                $@"SELECT
                    TOP(1)
                    mbak.[EncryptedKey],
                    pk.[Provider],
                    pk.[KeyName],
                    pk.[OaepPadding]
                FROM [dbo].[MessageBlobAccessKeys] mbak
                JOIN [dbo].[MessageBlobs] mb
                    ON [mbak].[MessageBlobId] = [mb].[MessageBlobId]
                JOIN [dbo].[ProfileKeys] pk ON mbak.[ProfileKeyId] = pk.[ProfileKeyId]
                WHERE mbak.[ProfileId] = @profileId
                    AND mb.[MessageId] = @messageId
                    AND mb.[BlobId] = @blobId";
            cmd.Parameters.AddWithValue("@profileId", profileId);
            cmd.Parameters.AddWithValue("@messageId", messageId);
            cmd.Parameters.AddWithValue("@blobId", blobId);
            cmd.Transaction = transaction;

            await using SqlDataReader reader = await cmd.ExecuteReaderAsync(ct);
            if (!(await reader.ReadAsync(ct)) ||
                (await reader.IsDBNullAsync(0, ct)))
            {
                throw new Exception("A MessageBlob with the provided (messageId, blobId) does not exist.");
            }

            byte[] encryptedKey = reader.GetSqlBytes(0).Buffer
                ?? throw new Exception("EncryptedKey should not be null.");
            string provider = reader.GetString(1);
            string keyName = reader.GetString(2);
            string oaepPadding = reader.GetString(3);

            return (
                encryptedKey,
                provider,
                keyName,
                oaepPadding
            );
        }

        private async Task CopyBlobContentToStreamInternalAsync(
            SqlConnection connection,
            SqlTransaction transaction,
            byte[] key,
            int blobId,
            Stream destination,
            int bufferSize,
            long? offset,
            long? length,
            CancellationToken ct)
        {
            (string path, byte[] transactionContext, byte[] IV) =
                await this.GetBlobAsync(connection, transaction, blobId, ct);

            var encryptor = this.encryptorFactory.CreateEncryptor(key, IV);

            using (SqlFileStream sqlFileStream =
                new(
                    path,
                    transactionContext,
                    FileAccess.Read,
                    FileOptions.SequentialScan,
                    allocationSize: 0))
            using (CryptoStream decryptionStream =
                new(
                    sqlFileStream,
                    encryptor.CreateDecryptor(),
                    CryptoStreamMode.Read))
            {
                if (offset != null && length != null)
                {
                    // TODO: if the streams are seekable use Stream.Seek

                    // read in Stream.Null the first 'offset' number of bytes
                    await decryptionStream.CopyToAsync(
                        Stream.Null,
                        bufferSize,
                        offset.Value,
                        ct);

                    await decryptionStream.CopyToAsync(
                        destination,
                        bufferSize,
                        length.Value,
                        ct);
                }
                else
                {
                    await decryptionStream.CopyToAsync(destination, bufferSize, ct);
                }
            }

            await transaction.CommitAsync(ct);
        }

        private async Task<(string path, byte[] transactionContext, byte[] IV)>
            GetBlobAsync(
                SqlConnection connection,
                SqlTransaction transaction,
                int blobId,
                CancellationToken ct)
        {
            string path;
            byte[] transactionContext;
            byte[] IV;

            await using (SqlCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText =
                    $@"SELECT
                        [EncryptedContent].PathName(),
                        GET_FILESTREAM_TRANSACTION_CONTEXT(),
                        [IV]
                    FROM [dbo].[Blobs]
                    WHERE BlobId = @blobId";
                cmd.Parameters.AddWithValue("@blobId", blobId);
                cmd.Transaction = transaction;

                await using SqlDataReader reader = await cmd.ExecuteReaderAsync(ct);
                if (!(await reader.ReadAsync(ct)) ||
                    (await reader.IsDBNullAsync(0, ct)))
                {
                    throw new Exception("A Blob with the provided blobId does not exist.");
                }

                path = reader.GetString(0);
                transactionContext = reader.GetSqlBytes(1).Buffer
                    ?? throw new Exception("TransactionContext should not be null.");
                IV = reader.GetSqlBytes(2).Buffer
                    ?? throw new Exception("IV should not be null.");
            }

            return (path, transactionContext, IV);
        }

        private async Task<long> GetUsedProfileStorageSpaceAsync(
            SqlConnection connection,
            int profileId,
            CancellationToken ct)
        {
            await using SqlCommand cmd = connection.CreateCommand();
            cmd.CommandText =
                $@"SELECT [UsedStorageSpace]
                   FROM [dbo].[ProfileStorageSpace_Indexed]
                   WHERE [ProfileId] = @profileId";
            cmd.Parameters.AddWithValue("@profileId", profileId);

            await using SqlDataReader reader = await cmd.ExecuteReaderAsync(ct);

            if (!reader.HasRows)
            {
                return 0;
            }

            await reader.ReadAsync(ct);

            return reader.GetInt64(reader.GetOrdinal("UsedStorageSpace"));
        }

        private async Task<long> GetProfileStorageQuotaAsync(
            SqlConnection connection,
            int profileId,
            CancellationToken ct)
        {
            await using SqlCommand cmd = connection.CreateCommand();
            cmd.CommandText =
                $@"SELECT [StorageQuotaInMb]
                   FROM [dbo].[ProfileQuotas]
                   WHERE [ProfileId] = @profileId";
            cmd.Parameters.AddWithValue("@profileId", profileId);

            await using SqlDataReader reader = await cmd.ExecuteReaderAsync(ct);

            if (!reader.HasRows)
            {
                return BlobConstants.DefaultStorageQuota;
            }

            await reader.ReadAsync(ct);

            int index = reader.GetOrdinal("StorageQuotaInMb");

            return reader.IsDBNull(index)
                ? BlobConstants.DefaultStorageQuota
                : Convert.ToInt64(reader.GetInt32(index)) * 1024 * 1024;
        }
    }
}

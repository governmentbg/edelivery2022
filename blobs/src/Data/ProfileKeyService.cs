using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using ED.Keystore;
using Google.Protobuf.WellKnownTypes;
using Medallion.Threading.SqlServer;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using static ED.Keystore.Keystore;

namespace ED.Blobs
{
    public class ProfileKeyService
    {
        private readonly KeystoreClient keystoreClient;
        private readonly string connectionString;
        private readonly TimeSpan profileKeyExpiration;

        public ProfileKeyService(
            KeystoreClient keystoreClient,
            IOptions<BlobsOptions> blobsOptionsAccessor,
            IOptions<DataOptions> dataOptionsAccessor)
        {
            this.keystoreClient = keystoreClient;
            this.profileKeyExpiration = blobsOptionsAccessor.Value.ProfileKeyExpiration;
            this.connectionString = dataOptionsAccessor.Value.GetConnectionString()
                ?? throw new Exception($"Missing setting {nameof(DataOptions)}.{nameof(DataOptions.ConnectionString)}");
        }

        public record ProfileKeyVO(
            int ProfileKeyId,
            DateTime ExpiresAt,
            string Provider,
            string KeyName,
            string OaepPadding);
        private async Task<ProfileKeyVO?> GetActiveProfileKeyAsync(
            SqlConnection connection,
            int profileId,
            CancellationToken ct)
        {
            await using SqlCommand cmd = connection.CreateCommand();

            cmd.CommandText =
                @"SELECT
                    [ProfileKeyId],
                    [ExpiresAt],
                    [Provider],
                    [KeyName],
                    [OaepPadding]
                FROM [dbo].[ProfileKeys]
                WHERE [ProfileId] = @profileId AND [IsActive] = 1";
            cmd.Parameters.Add(
                new SqlParameter("profileId", SqlDbType.Int)
                {
                    Value = profileId
                });

            await using SqlDataReader reader = await cmd.ExecuteReaderAsync(ct);
            if ((await reader.ReadAsync(ct)) && !(await reader.IsDBNullAsync(0, ct)))
            {
                int profileKeyId = reader.GetInt32(0);
                DateTime expiresAt = reader.GetDateTime(1);
                string provider = reader.GetString(2);
                string keyName = reader.GetString(3);
                string oaepPadding = reader.GetString(4);

                if ((await reader.ReadAsync(ct)) && !(await reader.IsDBNullAsync(0, ct)))
                {
                    throw new Exception("More than one active keys found");
                }

                return new ProfileKeyVO(
                    profileKeyId,
                    expiresAt,
                    provider,
                    keyName,
                    oaepPadding);
            }

            return null;
        }

        public async Task<ProfileKeyVO> GetOrCreateProfileKeyAsync(
                int profileId,
                CancellationToken ct)
        {
            // use a new connection as we dont wont the creation to be part
            // of the larger transaction
            await using SqlConnection connection = new(this.connectionString);
            await connection.OpenAsync(ct);

            // use the double checked lock pattern with a fast path
            // to ensure optimal performance without locking in the
            // most common scenario of an active key already existing

            ProfileKeyVO? profileKey =
                await this.GetActiveProfileKeyAsync(
                    connection,
                    profileId,
                    ct);

            // TODO: maybe use a period right before expiration instead
            // of waiting until the last second
            if (profileKey != null && profileKey.ExpiresAt > DateTime.Now)
            {
                // fast path, no locks required
                return profileKey;
            }

            var sqlLock = new SqlDistributedLock(
                $"GetOrCreateProfileKey:{profileId}",
                this.connectionString);
            // wait 30 seconds to acquire the lock and throw a TimeoutException if unable to
            await using var lockReleaseHandle = await sqlLock.AcquireAsync(TimeSpan.FromSeconds(30), ct);

            profileKey =
                await this.GetActiveProfileKeyAsync(
                    connection,
                    profileId,
                    ct);

            // TODO: maybe use a period right before expiration instead
            // of waiting until the last second
            if (profileKey != null && profileKey.ExpiresAt > DateTime.Now)
            {
                return profileKey;
            }

            CreateRsaKeyResponse newRsaKey =
                await this.keystoreClient.CreateRsaKeyAsync(
                    request: new Empty(),
                    cancellationToken: ct);

            DateTime issuedAt = DateTime.Now;
            DateTime expiresAt = issuedAt.Add(this.profileKeyExpiration);

            if (profileKey != null)
            {
                // deactivate the expired key

                await using SqlCommand cmdUpdate = connection.CreateCommand();

                cmdUpdate.CommandText =
                    @"UPDATE [dbo].[ProfileKeys]
                    SET [IsActive] = 0
                    FROM [dbo].[ProfileKeys]
                    WHERE [ProfileKeyId] = @profileKeyId";
                cmdUpdate.Parameters.Add(
                    new SqlParameter("profileKeyId", SqlDbType.Int)
                    {
                        Value = profileKey.ProfileKeyId
                    });

                await cmdUpdate.ExecuteNonQueryAsync(ct);
            }

            await using SqlCommand cmdInsert = connection.CreateCommand();

            cmdInsert.CommandText =
                @"INSERT INTO [dbo].[ProfileKeys] (
                    [ProfileId],
                    [Provider],
                    [KeyName],
                    [OaepPadding],
                    [IssuedAt],
                    [ExpiresAt],
                    [IsActive])
                OUTPUT
                    inserted.[ProfileKeyId]
                VALUES (
                    @profileId,
                    @provider,
                    @keyName,
                    @oaepPadding,
                    @issuedAt,
                    @expiresAt,
                    1)";
            cmdInsert.Parameters.AddRange(
                new[]
                {
                    new SqlParameter("profileId", SqlDbType.Int)
                    {
                        Value = profileId
                    },
                    new SqlParameter("provider", SqlDbType.NVarChar, 100)
                    {
                        Value = newRsaKey.Key.Provider
                    },
                    new SqlParameter("keyName", SqlDbType.NVarChar, 100)
                    {
                        Value = newRsaKey.Key.KeyName
                    },
                    new SqlParameter("oaepPadding", SqlDbType.NVarChar, 20)
                    {
                        Value = newRsaKey.Key.OaepPadding
                    },
                    new SqlParameter("issuedAt", SqlDbType.DateTime)
                    {
                        Value = issuedAt
                    },
                    new SqlParameter("expiresAt", SqlDbType.DateTime)
                    {
                        Value = expiresAt
                    },
                });

            int profileKeyId;
            await using (SqlDataReader reader = await cmdInsert.ExecuteReaderAsync(ct))
            {
                if (!(await reader.ReadAsync(ct)) || (await reader.IsDBNullAsync(0, ct)))
                {
                    throw new Exception("Inserting new profile key failed.");
                }

                profileKeyId = reader.GetInt32(0);
            }

            return new ProfileKeyVO(
                profileKeyId,
                expiresAt,
                newRsaKey.Key.Provider,
                newRsaKey.Key.KeyName,
                newRsaKey.Key.OaepPadding);
        }
    }
}

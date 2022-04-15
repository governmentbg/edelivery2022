using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Medallion.Threading.SqlServer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using static ED.Domain.IProfilesService;
using static ED.Keystore.Keystore;

namespace ED.Domain
{
    internal class ProfilesService : IProfilesService
    {
        private IServiceProvider serviceProvider;
        private IServiceScopeFactory scopeFactory;
        private KeystoreClient keystoreClient;
        private IOptions<DomainOptions> domainOptionsAccessor;

        public ProfilesService(
            IServiceProvider serviceProvider,
            IServiceScopeFactory scopeFactory,
            KeystoreClient keystoreClient,
            IOptions<DomainOptions> domainOptionsAccessor)
        {
            this.serviceProvider = serviceProvider;
            this.scopeFactory = scopeFactory;
            this.keystoreClient = keystoreClient;
            this.domainOptionsAccessor = domainOptionsAccessor;
        }

        public async Task<ProfileKeyVO> GetOrCreateProfileKeyAndSaveAsync(
            int profileId,
            CancellationToken ct)
        {
            // create a new scope to isolate the key creation
            using IServiceScope scope = this.scopeFactory.CreateScope();

            await using UnitOfWork unitOfWork =
                scope.ServiceProvider.GetRequiredService<UnitOfWork>();

            var profileServiceQueryRepository =
                scope.ServiceProvider.GetRequiredService<IProfileServiceQueryRepository>();

            // use the double checked lock pattern with a fast path
            // to ensure optimal performance without locking in the
            // most common scenario of an active key already existing

            var profileKey = await profileServiceQueryRepository
                .GetActiveProfileKeyAsync(profileId, ct);

            // TODO: maybe use a period right before expiration instead
            // of waiting until the last second
            if (profileKey != null && profileKey.ExpiresAt > DateTime.Now)
            {
                // fast path, no locks required
                return new ProfileKeyVO(
                    profileId,
                    profileKey.ProfileKeyId,
                    profileKey.Provider,
                    profileKey.KeyName,
                    profileKey.OaepPadding);
            }

            var sqlLock = new SqlDistributedLock(
                $"GetOrCreateProfileKey:{profileId}",
                this.domainOptionsAccessor.Value.GetConnectionString());
            // wait 30 seconds to acquire the lock and throw a TimeoutException if unable to
            await using var lockReleaseHandle = await sqlLock.AcquireAsync(TimeSpan.FromSeconds(30), ct);

            profileKey = await profileServiceQueryRepository
                .GetActiveProfileKeyAsync(profileId, ct);

            // TODO: maybe use a period right before expiration instead
            // of waiting until the last second
            if (profileKey != null && profileKey.ExpiresAt > DateTime.Now)
            {
                return new ProfileKeyVO(
                    profileId,
                    profileKey.ProfileKeyId,
                    profileKey.Provider,
                    profileKey.KeyName,
                    profileKey.OaepPadding);
            }

            // create the key upfront to minimize the transaction lifetime
            Keystore.CreateRsaKeyResponse newRsaKey =
                await this.keystoreClient.CreateRsaKeyAsync(
                    request: new Empty(),
                    cancellationToken: ct);

            DateTime issuedAt = DateTime.Now;
            DateTime expiresAt =
                issuedAt.Add(this.domainOptionsAccessor.Value.ProfileKeyExpiration);

            // wrap the deactivation of the expired
            // and the creation of the new key in a transaction
            await using ITransaction transaction =
               await unitOfWork.BeginTransactionAsync(ct);

            if (profileKey != null)
            {
                // deactivate the expired key
                await unitOfWork.DbContext.Database.ExecuteSqlRawAsync(
                    @"UPDATE [dbo].[ProfileKeys]
                    SET [IsActive] = 0
                    FROM [dbo].[ProfileKeys]
                    WHERE [ProfileKeyId] = @profileKeyId",
                    new[] { new SqlParameter("profileKeyId", profileKey.ProfileKeyId) },
                    ct);
            }

            // disregarding domain rules, just using EF to create a single ProfileKey
            ProfileKey newKey =
                new(
                    newRsaKey.Key.Provider,
                    newRsaKey.Key.KeyName,
                    newRsaKey.Key.OaepPadding,
                    issuedAt,
                    expiresAt)
                {
                    ProfileId = profileId
                };

            await unitOfWork.DbContext.AddAsync(newKey, ct);
            await unitOfWork.SaveAsync(ct);
            await transaction.CommitAsync(ct);

            return new ProfileKeyVO(
                profileId,
                newKey.ProfileKeyId,
                newRsaKey.Key.Provider,
                newRsaKey.Key.KeyName,
                newRsaKey.Key.OaepPadding);
        }

        public async Task<ProfileKeyVO> GetProfileKeyAsync(
            int profileKeyId,
            CancellationToken ct)
        {
            var profileServiceQueryRepository =
                this.serviceProvider.GetRequiredService<IProfileServiceQueryRepository>();

            var pk = await profileServiceQueryRepository
                .GetProfileKeyAsync(
                    profileKeyId,
                    ct);

            return new ProfileKeyVO(
                pk.ProfileId,
                pk.ProfileKeyId,
                pk.Provider,
                pk.KeyName,
                pk.OaepPadding);
        }
    }
}

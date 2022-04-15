using System;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.ClientFactory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ED.Keystore
{
    public class KeystoreService : Keystore.KeystoreBase
    {
        private readonly ILogger<KeystoreService> logger;
        private readonly GrpcClientFactory grpcClientFactory;
        private readonly CngKeystoreResolver cngKeystoreResolver;
        private readonly string cngProvider;
        private readonly (string deploymentName, string deploymentUrl, bool deploymentUseGrpcWeb)[] otherDeployments;
        public KeystoreService(
            ILogger<KeystoreService> logger,
            GrpcClientFactory grpcClientFactory,
            IOptions<KeystoreOptions> optionsAccessor,
            CngKeystoreResolver cngKeystoreResolver)
        {
            var options = optionsAccessor.Value;

            this.logger = logger;
            this.grpcClientFactory = grpcClientFactory;
            this.cngKeystoreResolver = cngKeystoreResolver;
            this.cngProvider =
                options.CngProvider
                ?? throw new Exception($"Missing setting {nameof(KeystoreOptions)}.{nameof(KeystoreOptions.CngProvider)}");
            this.otherDeployments = options.GetOtherDeployments();
        }

        public override async Task<CreateRsaKeyResponse> CreateRsaKey(
            Empty request,
            ServerCallContext context)
        {
            var keyName = $"ED:{Guid.NewGuid()}";
            var cngKeystore = this.cngKeystoreResolver(this.cngProvider);

            var key = cngKeystore.CreateRsaKey(keyName);

            if (this.otherDeployments.Length > 0)
            {
                byte[] keyData = cngKeystore.ExportRsaKey(key);

                // concurrently call all other deployments
                var keySyncTasks = this.otherDeployments
                    .Select(d =>
                        this.grpcClientFactory
                        .CreateClient<KeySync.KeySyncClient>(d.deploymentName)
                        .ImportRsaKeyAsync(
                            request: new ImportRsaKeyRequest
                            {
                                KeyName = keyName,
                                KeyData = ByteString.CopyFrom(keyData),
                            })
                        .ResponseAsync)
                    .ToArray();

                try
                {
                    await Task.WhenAll(keySyncTasks);
                }
#pragma warning disable CA1031 // catch a more specific allowed exception type, or rethrow the exception
                catch
#pragma warning restore CA1031
                {
                    // catch all exceptions as we'll await the tasks individually bellow
                }

                bool syncFailed = false;
                for (int i = 0; i < this.otherDeployments.Length; i++)
                {
                    try
                    {
                        await keySyncTasks[i];
                    }
                    catch (RpcException ex)
                    {
                        syncFailed = true;
                        this.logger.LogError(
                            ex,
                            $"Sync failed for '{this.otherDeployments[i].deploymentName}'");
                    }
                }

                if (syncFailed)
                {
                    throw new RpcException(
                        new Status(
                            StatusCode.Internal,
                            "Key synchronization failed."));
                }
            }

            return new CreateRsaKeyResponse
            {
                Key = new RsaKey
                {
                    Provider = key.Provider?.Provider
                        ?? throw new Exception("Provider should not be null"),
                    KeyName = keyName,
                    OaepPadding =
                        RsaUtils.FromRSAEncryptionPadding(cngKeystore.Padding),
                }
            };
        }

        public override Task<EncryptWithRsaKeyResponse> EncryptWithRsaKey(
            EncryptWithRsaKeyRequest request,
            ServerCallContext context)
        {
            GrpcUtils.AssertNotTypeDefault(
                nameof(request.Key.Provider),
                request.Key.Provider);
            GrpcUtils.AssertNotTypeDefault(
                nameof(request.Key.KeyName),
                request.Key.KeyName);
            GrpcUtils.AssertNotTypeDefault(
                nameof(request.Plaintext),
                request.Plaintext);

            var cngKeystore = this.cngKeystoreResolver(request.Key.Provider);
            var key = cngKeystore.OpenRsaKey(request.Key.KeyName);

            var encryptedData = RsaEncryptionHelper.EncryptRsa(
                key,
                cngKeystore.Padding,
                request.Plaintext.ToByteArray());

            return Task.FromResult(new EncryptWithRsaKeyResponse
            {
                EncryptedData = ByteString.CopyFrom(encryptedData)
            });
        }

        public override Task<DecryptWithRsaKeyResponse> DecryptWithRsaKey(
            DecryptWithRsaKeyRequest request,
            ServerCallContext context)
        {
            GrpcUtils.AssertNotTypeDefault(
                nameof(request.Key.Provider),
                request.Key.Provider);
            GrpcUtils.AssertNotTypeDefault(
                nameof(request.Key.KeyName),
                request.Key.KeyName);
            GrpcUtils.AssertNotTypeDefault(
                nameof(request.Key.OaepPadding),
                request.Key.OaepPadding);
            GrpcUtils.AssertNotTypeDefault(
                nameof(request.EncryptedData),
                request.EncryptedData);

            var cngKeystore = this.cngKeystoreResolver(request.Key.Provider);
            var key = cngKeystore.OpenRsaKey(request.Key.KeyName);

            var plaintext = RsaEncryptionHelper.DecryptRsa(
                key,
                RsaUtils.ToRSAEncryptionPadding(request.Key.OaepPadding),
                request.EncryptedData.ToByteArray());

            return Task.FromResult(new DecryptWithRsaKeyResponse
            {
                Plaintext = ByteString.CopyFrom(plaintext),
            });
        }
    }
}

using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ED.Keystore
{
    public class KeySyncService : KeySync.KeySyncBase
    {
        private readonly ILogger<KeySyncService> logger;
        private readonly ICngKeystore cngKeystore;
        public KeySyncService(
            ILogger<KeySyncService> logger,
            IOptions<KeystoreOptions> optionsAccessor,
            CngKeystoreResolver cngKeystoreResolver)
        {
            var options = optionsAccessor.Value;

            this.logger = logger;
            this.cngKeystore =
                cngKeystoreResolver(
                    options.CngProvider
                    ?? throw new Exception($"Missing setting {nameof(KeystoreOptions)}.{nameof(KeystoreOptions.CngProvider)}"));
        }

        public override Task<Empty> ImportRsaKey(
            ImportRsaKeyRequest request,
            ServerCallContext context)
        {
            GrpcUtils.AssertNotTypeDefault(nameof(request.KeyName), request.KeyName);
            GrpcUtils.AssertNotTypeDefault(nameof(request.KeyData), request.KeyData);

            this.cngKeystore.ImportRsaKey(
                request.KeyName,
                request.KeyData.ToByteArray());

            return Task.FromResult(new Empty());
        }
    }
}

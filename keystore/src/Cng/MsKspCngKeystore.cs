using System;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using Microsoft.Extensions.Logging;

namespace ED.Keystore
{
    public sealed class MsKspCngKeystore : ICngKeystore
    {
        private ILogger logger;

        public MsKspCngKeystore(ILogger<MsKspCngKeystore> logger)
        {
            this.logger = logger;
        }

        CngKey ICngKeystore.CreateRsaKey(string keyName) => CreateRsaKeyInternal(keyName);
        CngKey ICngKeystore.OpenRsaKey(string keyName) => OpenRsaKeyInternal(keyName, this.logger);
        byte[] ICngKeystore.ExportRsaKey(CngKey key) => ExportRsaKeyInternal(key);
        void ICngKeystore.ImportRsaKey(string keyName, byte[] keyData)
            => ImportRsaKeyInternal(keyName, keyData);
        RSAEncryptionPadding ICngKeystore.Padding => RSAEncryptionPadding.OaepSHA512;

        private const int DefaultRsaKeySize = 2048;
        private static readonly CngProvider Provider =
            CngProvider.MicrosoftSoftwareKeyStorageProvider;
        private static CngKeyCreationParameters GetDefaultKeyCreationParameters()
            => new()
            {
                KeyUsage = CngKeyUsages.AllUsages,
                KeyCreationOptions = CngKeyCreationOptions.MachineKey,
                ExportPolicy = CngExportPolicies.AllowPlaintextExport,
                Parameters =
                {
                    new CngProperty(
                        KeyStoragePropertyIdentifiers.NCRYPT_SECURITY_DESCR_PROPERTY,
                        GetDefaultKeyFileSecurity().GetSecurityDescriptorBinaryForm(),
                        CngPropertyOptions.Persist | DACL_SECURITY_INFORMATION)
                }
            };
        private static CngKeyOpenOptions DefaultKeyOpenOptions =
            CngKeyOpenOptions.MachineKey | CngKeyOpenOptions.Silent;

        private const CngPropertyOptions DACL_SECURITY_INFORMATION = (CngPropertyOptions)4;
        private static FileSecurity GetDefaultKeyFileSecurity()
        {
            FileSecurity acl = new();

            // Add the current user (this should be the AppPool Identity running the service)
            acl.AddAccessRule(
                new FileSystemAccessRule(
                    WindowsIdentity.GetCurrent().Name,
                    FileSystemRights.FullControl,
                    AccessControlType.Allow));

            // Add the default permissions for CNG keys - Administrators and SYSTEM
            acl.AddAccessRule(
                new FileSystemAccessRule(
                    new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null),
                    FileSystemRights.FullControl,
                    AccessControlType.Allow));
            acl.AddAccessRule(
                new FileSystemAccessRule(
                    new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null),
                    FileSystemRights.FullControl,
                    AccessControlType.Allow));

            return acl;
        }

        private static CngKey CreateRsaKeyInternal(string keyName)
        {
            CngKeyCreationParameters keyCreationParams =
                GetDefaultKeyCreationParameters();
            keyCreationParams.Provider = Provider;
            keyCreationParams.Parameters.Add(
                new CngProperty(
                    KeyStoragePropertyIdentifiers.NCRYPT_LENGTH_PROPERTY,
                    BitConverter.GetBytes(DefaultRsaKeySize),
                    CngPropertyOptions.None));

            return CngKey.Create(
                CngAlgorithm.Rsa,
                keyName,
                keyCreationParams);
        }

        private static CngKey OpenRsaKeyInternal(string keyName, ILogger logger)
        {
            CngKey key;
            try
            {
                key = CngKey.Open(keyName, Provider, DefaultKeyOpenOptions);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"OpenRsaKeyInternal failed for key {keyName}");
                throw;
            }

            if (key.Algorithm != CngAlgorithm.Rsa)
            {
                throw new Exception("The specified keyName is not an RSA key");
            }

            return key;
        }

        private static byte[] ExportRsaKeyInternal(CngKey key)
        {
            if (key.Algorithm != CngAlgorithm.Rsa)
            {
                throw new Exception("The specified keyName is not an RSA key");
            }

            return key.Export(CngKeyBlobFormat.GenericPrivateBlob);
        }

        private static void ImportRsaKeyInternal(string keyName, byte[] keyData)
        {
            CngKeyCreationParameters keyCreationParams =
                GetDefaultKeyCreationParameters();
            keyCreationParams.Provider = Provider;
            keyCreationParams.Parameters.Add(
                new CngProperty(
                    CngKeyBlobFormat.GenericPrivateBlob.Format,
                    keyData,
                    CngPropertyOptions.None));

            using var key =
                CngKey.Create(CngAlgorithm.Rsa, keyName, keyCreationParams);
        }
    }
}

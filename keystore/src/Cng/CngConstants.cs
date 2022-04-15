#pragma warning disable CA1707 // Identifiers should not contain underscores

namespace ED.Keystore
{
    // see https://docs.microsoft.com/en-us/windows/win32/seccng/key-storage-property-identifiers

    public static class KeyStoragePropertyIdentifiers
    {
        public const string NCRYPT_ALGORITHM_GROUP_PROPERTY = "Algorithm Group";
        public const string NCRYPT_ALGORITHM_PROPERTY = "Algorithm Name";
        public const string NCRYPT_ASSOCIATED_ECDH_KEY = "SmartCardAssociatedECDHKey";
        public const string NCRYPT_BLOCK_LENGTH_PROPERTY = "Block Length";
        public const string NCRYPT_CERTIFICATE_PROPERTY = "SmartCardKeyCertificate";
        public const string NCRYPT_DH_PARAMETERS_PROPERTY = "DHParameters";
        public const string NCRYPT_EXPORT_POLICY_PROPERTY = "Export Policy";
        public const string NCRYPT_IMPL_TYPE_PROPERTY = "Impl Type";
        public const string NCRYPT_KEY_TYPE_PROPERTY = "Key Type";
        public const string NCRYPT_KEY_USAGE_PROPERTY = "Key Usage";
        public const string NCRYPT_LAST_MODIFIED_PROPERTY = "Modified";
        public const string NCRYPT_LENGTH_PROPERTY = "Length";
        public const string NCRYPT_LENGTHS_PROPERTY = "Lengths";
        public const string NCRYPT_MAX_NAME_LENGTH_PROPERTY = "Max Name Length";
        public const string NCRYPT_NAME_PROPERTY = "Name";
        public const string NCRYPT_PIN_PROMPT_PROPERTY = "SmartCardPinPrompt";
        public const string NCRYPT_PIN_PROPERTY = "SmartCardPin";
        public const string NCRYPT_PROVIDER_HANDLE_PROPERTY = "Provider Handle";
        public const string NCRYPT_READER_PROPERTY = "SmartCardReader";
        public const string NCRYPT_ROOT_CERTSTORE_PROPERTY = "SmartcardRootCertStore";
        public const string NCRYPT_SCARD_PIN_ID = "SmartCardPinId";
        public const string NCRYPT_SCARD_PIN_INFO = "SmartCardPinInfo";
        public const string NCRYPT_SECURE_PIN_PROPERTY = "SmartCardSecurePin";
        public const string NCRYPT_SECURITY_DESCR_PROPERTY = "Security Descr";
        public const string NCRYPT_SECURITY_DESCR_SUPPORT_PROPERTY = "Security Descr Support";
        public const string NCRYPT_SMARTCARD_GUID_PROPERTY = "SmartCardGuid";
        public const string NCRYPT_UI_POLICY_PROPERTY = "UI Policy";
        public const string NCRYPT_UNIQUE_NAME_PROPERTY = "Unique Name";
        public const string NCRYPT_USE_CONTEXT_PROPERTY = "Use Context";
        public const string NCRYPT_USE_COUNT_ENABLED_PROPERTY = "Enabled Use Count";
        public const string NCRYPT_USE_COUNT_PROPERTY = "Use Count";
        public const string NCRYPT_USER_CERTSTORE_PROPERTY = "SmartCardUserCertStore";
        public const string NCRYPT_VERSION_PROPERTY = "Version";
        public const string NCRYPT_WINDOW_HANDLE_PROPERTY = "HWND Handle";
    }
}

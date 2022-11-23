namespace ED.Blobs
{
    public static class Policies
    {
        public const string WriteProfileBlob = "WriteProfileBlob";
        public const string WriteSystemRegistrationBlob = "WriteSystemRegistrationBlob";
        public const string WriteSystemTemplateBlob = "WriteSystemTemplateBlob";

        public const string EsbWriteProfileBlob = "EsbWriteProfileBlob";
        public const string EsbWriteProfileBlobOnBehalfOf = "EsbWriteProfileBlobOnBehalfOf";
    }

    public static class RequirementContextItems
    {
        public const string LoginId = "LoginId";
        public const string ProfileId = "ProfileId";
        public const string MessageId = "MessageId";
    }
}

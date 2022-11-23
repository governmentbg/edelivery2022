namespace ED.EsbApi;

public static class EsbAuthSchemeConstants
{
    public const string EsbAuthScheme = "EsbAuth";
    public const string DpMiscinfoHeader = "Dp-Miscinfo";
}

public static class EsbAuthClaimTypes
{
    public const string OId = "urn:esb:oid";
    public const string ClientId = "urn:esb:client_id";
    public const string RepresentedProfileIdentifier = "urn:esb:represented_profile_identifier";
    public const string OperatorId = "urn:esb:operator_id";

    public const string LoginId = "urn:esb:login_id";
    public const string OperatorLoginId = "urn:esb:operator_login_id";
    public const string RepresentedProfileId = "urn:esb:represented_profile_id";
}

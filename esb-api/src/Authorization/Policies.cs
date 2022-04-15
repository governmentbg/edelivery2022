namespace ED.EsbApi;

public static class Policies
{
    public const string ProfilesOnBehalfOf = "ProfilesOnBehalfOf";
    public const string ProfilesTargetGroupAccess = "TargetGroupAccess";
    public const string ProfilesIndividualTargetGroupAccess = "ProfilesIndividualTargetGroupAccess";

    public const string TemplateAccess = "TemplateAccess";

    public const string ReadMessageAsSender = "ReadMessageAsSender";
    public const string ReadMessageAsRecipient = "ReadMessageAsRecipient";
    public const string ReadInbox = "ReadInbox";
    public const string ReadOutbox = "ReadOutbox";
    public const string SendMessage = "SendMessage";
}

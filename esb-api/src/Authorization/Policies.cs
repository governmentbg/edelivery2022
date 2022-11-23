namespace ED.EsbApi;

public static class Policies
{
    public const string ProfilesTargetGroupAccess = "TargetGroupAccess";
    public const string ProfilesIndividualTargetGroupAccess = "ProfilesIndividualTargetGroupAccess";

    public const string TemplateAccess = "TemplateAccess";
    public const string OboTemplateAccess = "OboTemplateAccess";

    public const string ReadMessageAsSender = "ReadMessageAsSender";
    public const string ReadMessageAsRecipient = "ReadMessageAsRecipient";
    public const string SendMessage = "SendMessage";

    public const string OboProfilesAccess = "OboProfilesAccess";
    public const string OboReadInbox = "OboReadInbox";
    public const string OboReadOutbox = "OboReadOutbox";
    public const string OboSendMessage = "OboSendMessage";
}

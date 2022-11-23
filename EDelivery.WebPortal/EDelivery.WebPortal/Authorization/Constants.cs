namespace EDelivery.WebPortal.Authorization
{
    public static class Policies
    {
        public const string ReadMessageAsRecipient = "ReadMessageAsRecipient";
        public const string ReadMessageAsSender = "ReadMessageAsSender";
        public const string ReadMessageAsSenderOrRecipient = "ReadMessageAsSenderOrRecipient";
        public const string WriteMessage = "WriteMessage";
        public const string ForwardMessage = "ForwardMessage";
        public const string ReplyMessage = "ReplyMessage";
        public const string AdministerProfile = "AdministerProfile";
        public const string ListProfileMessage = "ListProfileMessage";
        public const string SearchMessageRecipients = "SearchMessageRecipients";
        public const string AdministerProfileRecipientGroups = "AdministerProfileRecipientGroups";
        public const string SearchMessageRecipientIndividuals = "SearchMessageRecipientIndividuals";
        public const string SearchMessageRecipientLegalEntities = "SearchMessageRecipientLegalEntities";
        public const string WriteCodeMessage = "WriteCodeMessage";
        public const string MessageAccess = "MessageAccess";
    }

    public static class RequirementContextItems
    {
        public const string LoginId = "LoginId";
        public const string ProfileId = "ProfileId";
        public const string TemplateId = "TemplateId";
        public const string MessageId = "MessageId";
        public const string RecipientGroupId = "RecipientGroupId";
        public const string ForwardingMessageId = "ForwardingMessageId";
    }
}

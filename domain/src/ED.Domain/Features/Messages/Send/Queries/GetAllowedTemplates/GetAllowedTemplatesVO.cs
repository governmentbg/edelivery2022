namespace ED.Domain
{
    public partial interface IMessageSendQueryRepository
    {
        public record GetAllowedTemplatesVO(
            int TemplateId,
            string Name);
    }
}

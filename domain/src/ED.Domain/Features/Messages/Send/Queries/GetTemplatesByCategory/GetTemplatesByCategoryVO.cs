namespace ED.Domain
{
    public partial interface IMessageSendQueryRepository
    {
        public record GetTemplatesByCategoryVO(
            int TemplateId,
            string Name);
    }
}

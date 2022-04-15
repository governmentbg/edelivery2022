namespace ED.Domain
{
    public partial interface IMessageOpenQueryRepository
    {
        public record GetTemplateContentVO(
            int TemplateId,
            string Content);
    }
}

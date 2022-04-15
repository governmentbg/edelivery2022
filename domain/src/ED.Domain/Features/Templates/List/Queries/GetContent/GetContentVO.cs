namespace ED.Domain
{
    public partial interface ITemplateListQueryRepository
    {
        public record GetContentVO(
            int TemplateId,
            string Content);
    }
}

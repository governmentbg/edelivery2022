namespace ED.Domain
{
    public partial interface IEsbTemplatesListQueryRepository
    {
        public record GetTemplateVO(
            int TemplateId,
            string Name,
            string IdentityNumber,
            int Read,
            int Write,
            string Content,
            int? ResponseTemplateId);
    }
}

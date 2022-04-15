namespace ED.Domain
{
    public partial interface IEsbTemplatesListQueryRepository
    {
        public record GetTemplatesVO(
            int TemplateId,
            string Name,
            string IdentityNumber,
            int Read,
            int Write,
            int? ResponseTemplateId);
    }
}

namespace ED.Domain
{
    public partial interface IProfileAdministerQueryRepository
    {
        public record GetAllowedTemplatesVO(
            int TemplateId,
            string Name);
    }
}

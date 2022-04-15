namespace ED.Domain
{
    public partial interface IAdminProfilesCreateEditViewQueryRepository
    {
        public record GetAllowedTemplatesVO(
            int TemplateId,
            string Name);
    }
}

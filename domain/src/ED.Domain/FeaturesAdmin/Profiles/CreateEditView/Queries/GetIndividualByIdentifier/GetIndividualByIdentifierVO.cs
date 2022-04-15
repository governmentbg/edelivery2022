namespace ED.Domain
{
    public partial interface IAdminProfilesCreateEditViewQueryRepository
    {
        public record GetIndividualByIdentifierVO(
            int LoginId,
            string LoginElectronicSubjectName);
    }
}

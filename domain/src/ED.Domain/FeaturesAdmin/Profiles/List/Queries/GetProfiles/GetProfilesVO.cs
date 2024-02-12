namespace ED.Domain
{
    public partial interface IAdminProfileListQueryRepository
    {
        public record GetProfilesVO(
            int ProfileId,
            ProfileType ProfileType,
            string Identifier,
            string ElectronicSubjectName,
            string Email,
            bool IsActivated,
            string TargetGroupName);
    }
}

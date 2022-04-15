namespace ED.Domain
{
    public partial interface IAdminProfilesCreateEditViewQueryRepository
    {
        public record GetProfileBasicInfoVO(
            string Identifier,
            int TargetGroupId,
            bool IsActivated);
    }
}

namespace ED.Domain
{
    public partial interface IAdminRegistrationsEditQueryRepository
    {
        public record GetProfileBasicInfoVO(
            int ProfileId,
            string Identifier,
            int TargetGroupId);
    }
}

namespace ED.Domain
{
    public partial interface IAdminNomenclaturesListQueryRepository
    {
        public record GetTargetGroupsByIdVO(
            int TargetGroupId,
            string TargetGroupName);
    }
}

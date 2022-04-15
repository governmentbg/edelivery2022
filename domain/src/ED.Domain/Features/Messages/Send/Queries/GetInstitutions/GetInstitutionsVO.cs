namespace ED.Domain
{
    public partial interface IMessageSendQueryRepository
    {
        public record GetInstitutionsVO(
            int ProfileId,
            string Name,
            int TargetGroup);
    }
}

namespace ED.Domain
{
    public partial interface IEsbMessagesSendQueryRepository
    {
        public record GetProfileNamesVO(
            int ProfileId,
            string ProfileName);
    }
}

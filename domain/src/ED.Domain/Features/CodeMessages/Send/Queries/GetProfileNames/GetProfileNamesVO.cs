namespace ED.Domain
{
    public partial interface ICodeMessageSendQueryRepository
    {
        public record GetProfileNamesVO(
            int ProfileId,
            string ProfileName);
    }
}

namespace ED.Domain
{
    public partial interface IProfileServiceQueryRepository
    {
        public record GetProfileKeyVO(
            int ProfileKeyId,
            int ProfileId,
            string Provider,
            string KeyName,
            string OaepPadding);
    }
}

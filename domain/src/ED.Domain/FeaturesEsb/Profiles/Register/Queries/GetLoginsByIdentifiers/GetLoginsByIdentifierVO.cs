namespace ED.Domain
{
    public partial interface IEsbProfilesRegisterQueryRepository
    {
        public record GetLoginsByIdentifiersVO(
            string Identifier,
            int LoginId);
    }
}

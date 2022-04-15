namespace ED.Domain
{
    public partial interface IEsbProfilesAuthenticateQueryRepository
    {
        public record GetEsbUserVO(
            int ProfileId,
            int LoginId,
            int? OperatorLoginId,
            int? RepresentedProfileId);
    }
}

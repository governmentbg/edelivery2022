namespace ED.Domain
{
    public partial interface IEsbProfilesListQueryRepository
    {
        public record GetTargetGroupProfilesVO(
            int ProfileId,
            string Identifier,
            string Name,
            string Email,
            string Phone);
    }
}

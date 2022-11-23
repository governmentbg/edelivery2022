namespace ED.Domain
{
    public partial interface IEsbProfilesListQueryRepository
    {
        public record SearchGetTargetGroupProfilesVO(
            int ProfileId,
            string Identifier,
            string Name,
            string Email,
            string Phone);
    }
}

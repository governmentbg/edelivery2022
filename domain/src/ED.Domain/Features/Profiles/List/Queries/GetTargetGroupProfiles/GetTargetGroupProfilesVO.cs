namespace ED.Domain
{
    public partial interface IProfileListQueryRepository
    {
        public record GetTargetGroupProfilesVO(
            string Name,
            string Identifier);
    }
}

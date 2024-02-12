namespace ED.Domain
{
    public partial interface IEsbCountriesListQueryRepository
    {
        public record GetCountriesVO(
            string Iso,
            string Name);
    }
}

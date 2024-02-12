namespace ED.Domain
{
    partial class EsbCountriesListQueryRepository : Repository, IEsbCountriesListQueryRepository
    {
        public EsbCountriesListQueryRepository(
            UnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
    }
}

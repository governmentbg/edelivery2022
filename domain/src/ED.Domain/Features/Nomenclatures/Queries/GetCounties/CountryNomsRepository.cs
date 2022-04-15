namespace ED.Domain
{
    partial class CountryNomsRepository : EntityCodeNomsRepository<Country, EntityCodeNomVO>, ICountryNomsRepository
    {
        public CountryNomsRepository(UnitOfWork unitOfWork)
            : base(
                  unitOfWork,
                  e => e.CountryISO2,
                  e => e.Name,
                  e => new EntityCodeNomVO(e.CountryISO2, e.Name))
        {
        }
    }
}

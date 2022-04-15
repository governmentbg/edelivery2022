namespace ED.Domain
{
#pragma warning disable CA1040 // Avoid empty interfaces
    public partial interface ICountryNomsRepository : INomsRepository<Country, string, EntityCodeNomVO>
#pragma warning restore CA1040 // Avoid empty interfaces
    {
    }
}

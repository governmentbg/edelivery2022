using ED.DomainServices.Admin;
using Mapster;
using static ED.Domain.IAdminProfilesCreateEditViewQueryRepository;

namespace ED.DomainServices
{
    public class OneOfFieldMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // skip null value mapping as it interferes with "oneof" fields
            config.ForType<GetProfileInfoVO, GetProfileInfoResponse>().IgnoreNullValues(true);
            config.ForType<GetProfileDataVO, GetProfileDataResponse>().IgnoreNullValues(true);
        }
    }
}

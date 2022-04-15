using System;
using Mapster;

namespace ED.DomainServices
{
    public class GuidMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.ForType<Guid, string>()
                .MapWith(src => src.ToString().ToUpperInvariant());
        }
    }
}

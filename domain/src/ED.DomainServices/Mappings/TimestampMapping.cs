using System;
using Mapster;

namespace ED.DomainServices
{
    public class TimestampMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.ForType<DateTime, Google.Protobuf.WellKnownTypes.Timestamp>()
                .MapWith(src => src.ToTimestamp());

            config.ForType<DateTime?, Google.Protobuf.WellKnownTypes.Timestamp?>()
                .MapWith(src => src.HasValue ? src.Value.ToTimestamp() : null);
        }
    }
}

using System;
using Mapster;

namespace ED.EsbApi;

public class TimestampMapping : IRegister
{
    private DateTime ConvertToDateTime(
        Google.Protobuf.WellKnownTypes.Timestamp timestamp)
    {
        return timestamp.ToLocalDateTime();
    }

    private DateTime? ConvertToNullableDateTime(
        Google.Protobuf.WellKnownTypes.Timestamp? timestamp)
    {
        return timestamp == null
            ? null
            : this.ConvertToDateTime(timestamp);
    }

    public void Register(TypeAdapterConfig config)
    {
        config.ForType<Google.Protobuf.WellKnownTypes.Timestamp, DateTime>()
               .MapWith(src => this.ConvertToDateTime(src));
               
        config.ForType<Google.Protobuf.WellKnownTypes.Timestamp?, DateTime?>()
               .MapWith(src => this.ConvertToNullableDateTime(src));
    }
}

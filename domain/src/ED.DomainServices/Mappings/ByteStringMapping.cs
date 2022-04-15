using Mapster;

namespace ED.DomainServices
{
    public class ByteStringMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.ForType<byte[], Google.Protobuf.ByteString>()
                .MapWith(src => Google.Protobuf.ByteString.CopyFrom(src));

            config.ForType<byte[]?, Google.Protobuf.ByteString?>()
                .MapWith(src => src == null
                    ? Google.Protobuf.ByteString.Empty
                    : Google.Protobuf.ByteString.CopyFrom(src!));
        }
    }
}

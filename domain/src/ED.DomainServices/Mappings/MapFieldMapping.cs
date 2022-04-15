using Mapster;

namespace ED.DomainServices
{
    public class MapFieldMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config
                .Default
                .UseDestinationValue(
                    member => member.SetterModifier == AccessModifier.None
                    && member.Type.IsGenericType
                    && member.Type.GetGenericTypeDefinition() == typeof(Google.Protobuf.Collections.MapField<,>));
        }
    }
}

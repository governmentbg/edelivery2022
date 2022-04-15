using System.Runtime.Serialization;

namespace EDelivery.WebPortal.Enums
{
    [DataContract]
    public enum eTimeStampType
    {
        [EnumMember]
        Document,
        [EnumMember]
        NRO,
        [EnumMember]
        NRD
    }
}

using System.Runtime.Serialization;

namespace EDelivery.SEOS.DataContracts
{
    [DataContract]
    public enum EntityServiceStatusEnum
    {
        [EnumMemberAttribute]
        Inactive,
        [EnumMemberAttribute]
        Active,
        [EnumMemberAttribute]
        TemporarilyInactive
    }
}

using System.Runtime.Serialization;

namespace EDelivery.SEOS.DataContracts
{
    [DataContract]
    public enum SortOrderEnum
    {
        [EnumMemberAttribute]
        None,
        [EnumMemberAttribute]
        Asc,
        [EnumMemberAttribute]
        Desc
    }
}

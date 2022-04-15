using System.Runtime.Serialization;

namespace EDelivery.SEOS.DataContracts
{
    [DataContract]
    public enum SortColumnEnum
    {
        [EnumMemberAttribute]
        None,
        [EnumMemberAttribute]
        Status,
        [EnumMemberAttribute]
        ReceiverName,
        [EnumMemberAttribute]
        Title,
        [EnumMemberAttribute]
        SenderName,
        [EnumMemberAttribute]
        DateSent,
        [EnumMemberAttribute]
        DateReceived,
        [EnumMemberAttribute]
        RegIndex,
        [EnumMemberAttribute]
        DocKind,
        [EnumMemberAttribute]
        DocReferenceNumber
    }
}

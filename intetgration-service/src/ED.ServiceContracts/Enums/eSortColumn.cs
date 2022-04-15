using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.Common.Enums
{
    [DataContract]
    public enum eSortColumn
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
        DocReferenceNumber
    }
}

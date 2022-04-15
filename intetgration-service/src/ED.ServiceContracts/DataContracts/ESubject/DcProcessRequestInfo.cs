using System.Runtime.Serialization;

using EDelivery.Common.Enums;

namespace EDelivery.Common.DataContracts.ESubject
{
    [DataContract]
    public class DcProcessRequestInfo
    {
        [DataMember]
        public eRequestStatus Status { get; set; }
        [DataMember]
        public string Comment { get; set; }
    }
}

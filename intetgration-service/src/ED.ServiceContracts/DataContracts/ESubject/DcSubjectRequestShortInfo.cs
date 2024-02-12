using EDelivery.Common.Enums;
using System;
using System.Runtime.Serialization;

namespace EDelivery.Common.DataContracts.ESubject
{
    /// <summary>
    /// Information for requests list
    /// </summary>
    [DataContract]
    public class DcSubjectRequestShortInfo
    {
        [DataMember]
        public int RequestId { get; set; }
        [DataMember]
        public eRequestType RequestType { get; set; }
        [DataMember]
        public DateTime RequestDate { get; set; }
        [DataMember]
        public string RequestedForSubjectName { get; set; }
        [DataMember]
        public Guid RequestedForSubjectId { get; set; }
        [DataMember]
        public string RequestedByName { get; set; }
        [DataMember]
        public eRequestStatus RequestStatus { get; set; }
        [DataMember]
        public DateTime? ProcessDate { get; set; }
        [DataMember]
        public string ProcessedBy { get; set; }

    }
}

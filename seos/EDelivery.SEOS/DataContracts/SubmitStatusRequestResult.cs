using System.Runtime.Serialization;

namespace EDelivery.SEOS.DataContracts
{
    [DataContract]
    public class SubmitStatusRequestResult
    {
        [DataMember]
        public DocumentStatusType Status { get; set; }
        [DataMember]
        public DocumentStatusResponseType StatusResponse { get; set; }
        [DataMember]
        public string Error { get; set; }
        [DataMember]
        public string StatusResponseMessage { get; set; }
    }
}

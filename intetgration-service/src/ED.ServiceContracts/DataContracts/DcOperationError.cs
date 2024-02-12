using System.Runtime.Serialization;

namespace EDelivery.Common.DataContracts
{
    [DataContract]
    public class DcOperationError
    {
        [DataMember]
        public Common.Enums.eErrorCode ErrorCode { get; set; }

        [DataMember]
        public string ErrorMessage { get; set; }
    }
}

using System.Runtime.Serialization;

namespace EDelivery.Common.DataContracts
{
    [DataContract]
    public class DcEFormsRequestedService
    {
        [DataMember]
        public string MessageTitle { get; set; }
        [DataMember]
        public int Count { get; set; }
    }
}

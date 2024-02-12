using System.Runtime.Serialization;

namespace EDelivery.Common.DataContracts
{
    [DataContract]
    public class DcProfileInfo : DcProfile
    {
        [DataMember]
        public int NewMessagesCount { get; set; }
    }
}

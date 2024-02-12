using System.Runtime.Serialization;

namespace EDelivery.Common.DataContracts
{
    [DataContract]
    public class DcProfileWithNewMsgCount : DcProfile
    {
        [DataMember]
        public int NewMessages { get; set; }
        [DataMember]
        public bool IsInstitution { get; set; }
        [DataMember]
        public bool IsSendMessageWithCodeEnabled { get; set; }
        [DataMember]
        public string UniqueIdentifier { get; set; }
        [DataMember]
        public bool IsReadOnly { get; set; }

        public bool HasSEOSRegistration { get; set; }

        public int NewSEOSMessages { get; set; }
    }
}

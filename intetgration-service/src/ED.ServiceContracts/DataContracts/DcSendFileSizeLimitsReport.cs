using System.Runtime.Serialization;


namespace EDelivery.Common.DataContracts
{
    [DataContract]
    public class DcSendFileSizeLimitsReport
    {
        [DataMember]
        public string ElectronicSubjectName { get; set; }

        [DataMember]
        public string UniqueSubjectIdentifier { get; set; }

        [DataMember]
        public int? MaxSendDocSizeInMb { get; set; }

        [DataMember]
        public int? MaxReceiveDocSizeInMb { get; set; }
    }
}

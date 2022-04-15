using System;
using System.Runtime.Serialization;

namespace EDelivery.Common.DataContracts
{
    [DataContract]
    public class DcEFormsMessageReport
    {
        [DataMember]
        public string MessageTitle { get; set; }

        [DataMember]
        public DateTime DateSent { get; set; }

        [DataMember]
        public string ElectronicSubjectName { get; set; }

        [DataMember]
        public string UniqueSubjectIdentifier { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EDelivery.Common.DataContracts.ESubject
{
    [DataContract]
    public class WebInstitutionInfo : DcInstitutionInfo
    {
        public WebInstitutionInfo() : base()
        { }

        [DataMember]
        public int Type { get; set; }
        [DataMember]
        public bool IsReadOnly { get; set; }
        [DataMember]
        public bool IsSendMessageWithCodeEnabled { get; set; }
        [DataMember]
        public DateTime? DateDeleted { get; set; }
        [DataMember]
        public DcDocument RegistrationDocument { get; set; }
        [DataMember]
        public List<DcDocumentAdditional> AdditionalDcouments { get; set; }
    }
}

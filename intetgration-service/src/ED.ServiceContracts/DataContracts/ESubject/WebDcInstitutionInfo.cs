using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EDelivery.Common.DataContracts.ESubject
{
    [DataContract]
    public class WebDcInstitutionInfo : DcInstitutionInfo
    {
        public WebDcInstitutionInfo() : base()
        { }

        [DataMember]
        public int Type { get; set; }
        [DataMember]
        public bool IsReadOnly { get; set; }
        [DataMember]
        public DcDocument RegistrationDocument { get; set; }
        [DataMember]
        public List<DcDocumentAdditional> AdditionalDcouments { get; set; }
    }
}

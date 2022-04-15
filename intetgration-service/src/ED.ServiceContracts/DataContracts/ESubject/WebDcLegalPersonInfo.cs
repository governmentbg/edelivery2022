using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EDelivery.Common.DataContracts.ESubject
{
    [DataContract]
    public class WebDcLegalPersonInfo : DcLegalPersonInfo
    {
        [DataMember]
        public List<DcDocument> RegistrationDcouments { get; set; }
    }
}

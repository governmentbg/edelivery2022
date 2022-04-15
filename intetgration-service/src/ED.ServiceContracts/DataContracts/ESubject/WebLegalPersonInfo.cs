using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EDelivery.Common.DataContracts.ESubject
{
    [DataContract]
    public class WebLegalPersonInfo : DcLegalPersonInfo
    {
        [DataMember]
        public List<DcDocument> RegistrationDcouments { get; set; }
        [DataMember]
        public DateTime? DateDeleted { get; set; }
    }
}

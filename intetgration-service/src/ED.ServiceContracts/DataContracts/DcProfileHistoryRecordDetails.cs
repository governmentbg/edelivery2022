using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace EDelivery.Common.DataContracts
{
    [DataContract]
    public class DcProfileHistoryRecordDetails
    {
        [DataMember]
        public string RelatedUserName { get; set; }

        [DataMember]
        public Guid RelatedUserGuid { get; set; }

        [DataMember]
        public string RelatedData { get; set; }
    }
}

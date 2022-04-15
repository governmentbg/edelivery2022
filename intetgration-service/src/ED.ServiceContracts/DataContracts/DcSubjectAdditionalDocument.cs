using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.Common.DataContracts
{
    [DataContract]
     public class DcSubjectAdditionalDocument: DcDocument
    {
        [DataMember]
        public Guid ElectronicSubjectId { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public DateTime CreatedOn { get; set; }
        [DataMember]
        public string CreatedBy { get; set; }
    }
}

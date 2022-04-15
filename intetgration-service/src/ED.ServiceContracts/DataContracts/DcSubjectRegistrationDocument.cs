using EDelivery.Common.DataContracts.ESubject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.Common.DataContracts
{
    [DataContract]
    public class DcSubjectRegistrationDocument:DcDocument
    {
        [DataMember]
        public Guid? ElectronicSubjectId { get; set; }
        [DataMember]
        public DateTime CreatedOn { get; set; }
        [DataMember]
        public DcSubjectRequestShortInfo DataChangeRequest { get; set; }

    }
}

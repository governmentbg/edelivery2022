using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.Common.DataContracts
{
    [DataContract]
    public class DcLoginAccess
    {
        [DataMember]
        public int LoginId { get; set; }

        [DataMember]
        public int AccessGrantedById { get; set; }

        [DataMember]
        public string LoginName { get; set; }

        [DataMember]
        public string AccessGrantedByName { get; set; }

        [DataMember]
        public DateTime AccessGrantedDate { get; set; }
    }
}

using EDelivery.Common.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.Common.DataContracts
{
    [DataContract]
    public class DcDocumentSize
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public long SizeInBytes { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace EDelivery.Common.DataContracts
{
    [DataContract]
    public class DcChainCertificate
    {
        [DataMember]
        public string Subject { get; set; }

        [DataMember]
        public bool IsRoot { get; set; }
    }
}

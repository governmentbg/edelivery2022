using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.Common.DataContracts
{
    [DataContract]
    public class DcIntegrationDetails
    {
        [DataMember]
        public bool Enabled { get; set; }
        [DataMember]
        public int ProfileId { get; set; }
        [DataMember]
        public string CertificateThumbprint { get; set; }
        [DataMember]
        public string PushNotificaitonsUrl { get; set; }
    }
}

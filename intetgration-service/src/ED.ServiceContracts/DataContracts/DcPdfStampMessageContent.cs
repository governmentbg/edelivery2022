using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.Common.DataContracts
{
    [DataContract]
    public class DcPdfStampMessageContent
    {
        [DataMember]
        public string FileName { get; set; }

        [DataMember]
        public string ContentType { get; set; }

        [DataMember]
        public byte[] Content { get; set; }
    }
}

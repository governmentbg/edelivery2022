using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.Common.DataContracts
{
    [DataContract]
    public class DcTimeStamp
    {
        public DcTimeStamp()
        {

        }

        public DcTimeStamp(string fileName, byte[] content)
        {
            this.FileName = fileName;
            TimeStampData = content;
        }

        [DataMember]
        public string FileName { get; set; }
        [DataMember]
        public byte[] TimeStampData { get; set; }
    }
}

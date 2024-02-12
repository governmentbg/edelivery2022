using System.Runtime.Serialization;

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
            this.TimeStampData = content;
        }

        [DataMember]
        public string FileName { get; set; }
        [DataMember]
        public byte[] TimeStampData { get; set; }
    }
}

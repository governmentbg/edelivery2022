using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EDelivery.Common.DataContracts
{
    [DataContract]
    public class WebDcMessageDetails : WebDcMessage
    {
        public WebDcMessageDetails()
        {
        }

        public WebDcMessageDetails(WebDcMessage webDcMessage) : base(webDcMessage)
        {
        }

        public WebDcMessageDetails(DcMessageDetails message) : base(message)
        {
            this.MessageText = message.MessageText;
            this.AttachedDocuments = message.AttachedDocuments;
            this.TimeStampNRO = message.TimeStampNRO;
            this.TimeStampNRD = message.TimeStampNRD;
            this.TimeStampContent = message.TimeStampContent;
            this.FirstTimeOpen = message.FirstTimeOpen;
        }

        [DataMember]
        public string MessageText { get; set; }

        [DataMember]
        public List<DcDocument> AttachedDocuments { get; set; }

        [DataMember]
        public DcTimeStamp TimeStampNRO { get; set; }

        [DataMember]
        public DcTimeStamp TimeStampNRD { get; set; }

        [DataMember]
        public DcTimeStampMessageContent TimeStampContent { get; set; }

        public bool FirstTimeOpen { get; set; }
    }
}

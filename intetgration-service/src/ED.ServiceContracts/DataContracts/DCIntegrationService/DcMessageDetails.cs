using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.Common.DataContracts
{
    [DataContract]
    public class DcMessageDetails:DcMessage
    {
        public DcMessageDetails()
        {

        }

        public DcMessageDetails(WebDcMessageDetails message):base(message)
        {
            if (message == null) return;
            this.MessageText = message.MessageText;
            this.AttachedDocuments = message.AttachedDocuments;
            this.TimeStampNRO = message.TimeStampNRO;
            this.TimeStampNRD = message.TimeStampNRD;
            this.TimeStampContent = message.TimeStampContent;
            this.FirstTimeOpen = message.FirstTimeOpen;
            if(message.ForwardedMessage!=null && message.ForwardStatus == Enums.eForwardStatus.IsInForwardChain)
            {
                this.MessageText = $"{message.ForwardedMessage.MessageText}\r\n Препратено от {message.SenderProfile.ElectronicSubjectName} с текст: {message.MessageText}";
                this.AttachedDocuments = message.ForwardedMessage.AttachedDocuments;
            }
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

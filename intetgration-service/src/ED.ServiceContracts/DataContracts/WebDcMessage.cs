using EDelivery.Common.Enums;
using System;
using System.Runtime.Serialization;

namespace EDelivery.Common.DataContracts
{
    [DataContract]
    public class WebDcMessage
    {
        public WebDcMessage()
        {
        }

        public WebDcMessage(WebDcMessage message)
        {
            this.Id = message.Id;
            this.Title = message.Title;
            this.IsDraft = message.IsDraft;
            this.DateCreated = message.DateCreated;
            this.DateSent = message.DateSent;
            this.DateReceived = message.DateReceived;
            this.SenderLogin = message.SenderLogin;
            this.ReceiverLogin = message.ReceiverLogin;
            this.SenderProfile = message.SenderProfile;
            this.ReceiverProfile = message.ReceiverProfile;
            this.ForwardStatus = message.ForwardStatus;
        }

        public WebDcMessage(DcMessageDetails message)
        {
            this.Id = message.Id;
            this.Title = message.Title;
            this.IsDraft = message.IsDraft;
            this.DateCreated = message.DateCreated;
            this.DateSent = message.DateSent;
            this.DateReceived = message.DateReceived;
            this.SenderLogin = message.SenderLogin;
            this.ReceiverLogin = message.ReceiverLogin;
            this.SenderProfile = message.SenderProfile;
            this.ReceiverProfile = message.ReceiverProfile;
        }


        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public bool IsDraft { get; set; }

        [DataMember]
        public DateTime DateCreated { get; set; }

        [DataMember]
        public DateTime? DateSent { get; set; }

        [DataMember]
        public DateTime? DateReceived { get; set; }

        [DataMember]
        public DcLogin SenderLogin { get; set; }

        [DataMember]
        public DcLogin ReceiverLogin { get; set; }

        [DataMember]
        public DcProfile SenderProfile { get; set; }

        [DataMember]
        public DcProfile ReceiverProfile { get; set; }

        [DataMember]
        public eForwardStatus ForwardStatus { get; set; }

        [DataMember]
        public WebDcMessageDetails ForwardedMessage { get; set; }
    }
}

using System;

namespace EDelivery.SEOS.Models
{
    public struct MsgData
    {
        public String l1Timestamp;
        public String l1MessageId;
        public String l1From;
        public String l1To;
        public String l2Timestamp;
        public String l2MessageId;
        public String l2From;
        public String l2To;
        public String originalSender;
        public String finalRecepient;
        public String originalSenderType;
        public String finalRecepientType;
        public String payload;
    }
}

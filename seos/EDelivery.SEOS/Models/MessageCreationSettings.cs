using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDelivery.SEOS.DataContracts;

namespace EDelivery.SEOS.Models
{
    public class MessageCreationSettings
    {
        public string MessageGuid { get; set; }
        public DocumentIdentificationType DocIdentity { get; set; }
        public string RejectionReason { get; set; }
        public bool DocExpectCloseDateSpecified { get; set; }
        public DateTime DocExpectCloseDate { get; set; }
        public EntityNodeType Sender { get; set; }
        public EntityNodeType Receiver { get; set; }
    }
}

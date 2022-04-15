using System;
using System.Collections.Generic;
using EDelivery.SEOS.DBEntities;

namespace EDelivery.SEOS.Models
{
    public class MessagesPage
    {
        public List<SEOSMessage> Messages { get; set; }
        public int CountAllMessages { get; set; }
    }
}

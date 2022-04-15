using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDelivery.WebPortal
{
    public class InfoMessage:Exception
    {
        public InfoMessage(string message):base(message)
        {}
    }
}
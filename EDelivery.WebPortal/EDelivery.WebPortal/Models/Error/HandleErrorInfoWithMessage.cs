using System;
using System.Web.Mvc;

namespace EDelivery.WebPortal.Models
{
    public class HandleErrorInfoWithMessage : HandleErrorInfo
    {
        public HandleErrorInfoWithMessage(string message) :
            base(new Exception(message), "Home", "Index")
        {
            this.ErrorMessage = message;
        }
        public string ErrorMessage { get; set; }
    }
}
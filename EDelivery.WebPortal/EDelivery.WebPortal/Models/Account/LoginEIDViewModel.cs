using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace EDelivery.WebPortal.Models
{
    /// <summary>
    /// Model for submitting eid request
    /// </summary>
    public class LoginEIDViewModel
    {
        public LoginEIDViewModel()
        {
            this.Target = "BGeID";
            this.OA = ConfigurationManager.AppSettings["bg.gov.eid.OAUrl"];
            this.MoccaURL = ConfigurationManager.AppSettings["bg.gov.eid.moccaUrl"];
            this.TemplateURL = ConfigurationManager.AppSettings["bg.gov.eid.templateUrl"];
            this.StartAuthenticationURL = ConfigurationManager.AppSettings["bg.gov.eid.startAuthenticataionUrl"];

            if(string.IsNullOrEmpty(this.OA)||string.IsNullOrEmpty(this.MoccaURL)||string.IsNullOrEmpty(this.TemplateURL)||string.IsNullOrEmpty(StartAuthenticationURL))
            {
                throw new ConfigurationErrorsException("EID Validation is not configured in Web.Config!");
            }
        }

        public string StartAuthenticationURL { get; set; }

        public string OA { get; set; }

        public string Target { get; set; }

        public string MoccaURL { get; set; }

        public string TemplateURL { get; set; }
    }
}
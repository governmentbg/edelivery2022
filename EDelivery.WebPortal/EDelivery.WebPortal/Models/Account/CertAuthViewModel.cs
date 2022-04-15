using System.Configuration;
using EDelivery.WebPortal.Utils;

namespace EDelivery.WebPortal.Models
{
    /// <summary>
    /// Model for submitting eauth request (external modul)
    /// </summary>
    public class CertAuthViewModel
    {
        public const string RelayStateLogin = "login";

        public CertAuthViewModel()
        {
            this.ReturnUrl = SamlHelper.SamlConfiguration.ReturnUrl;
            this.TargetUrl = SamlHelper.SamlConfiguration.TargetUrl;
            this.ProviderName = SamlHelper.SamlConfiguration.ProviderName;
            this.ProviderId = SamlHelper.SamlConfiguration.ProviderId;
            this.ExtServiceId = SamlHelper.SamlConfiguration.ExtServiceId;
            this.ExtProviderId = SamlHelper.SamlConfiguration.ExtProviderId;

            if (string.IsNullOrEmpty(this.TargetUrl) || string.IsNullOrEmpty(this.ReturnUrl) ||
                string.IsNullOrEmpty(this.ProviderName) || string.IsNullOrEmpty(ProviderId))
            {
                throw new ConfigurationErrorsException("eAuthenticator is not configured in Web.Config!");
            }
        }

        public string ReturnUrl { get; set; }

        public string TargetUrl { get; set; }

        public string ProviderName { get; set; }

        public string ProviderId { get; set; }
        
        public string ExtServiceId { get; set; }

        public string ExtProviderId { get; set; }

        public string EncodedRequest { get; set; }

        public string EncodedRelayState { get; set; }
    }
}

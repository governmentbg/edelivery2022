using System.Security.Cryptography;
using Microsoft.Owin.Security.DataProtection;

namespace EDelivery.WebPortal
{
    public class LocalMachineDpapiDataProtectionProvider : IDataProtectionProvider
    {
        public IDataProtector Create(params string[] purposes)
        {
            return new LocalMachineDpapiDataProtector(purposes);
        }
    }
}
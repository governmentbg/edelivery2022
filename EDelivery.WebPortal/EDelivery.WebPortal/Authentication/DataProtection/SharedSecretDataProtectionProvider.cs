using Microsoft.Owin.Security.DataProtection;

namespace EDelivery.WebPortal
{
    internal class SharedSecretDataProtectionProvider : IDataProtectionProvider
    {
        private readonly string key;

        public SharedSecretDataProtectionProvider(string key)
        {
            this.key = key;
        }

        public IDataProtector Create(params string[] purposes)
        {
            return new SharedSecretDataProtector(this.key, purposes);
        }
    }
}

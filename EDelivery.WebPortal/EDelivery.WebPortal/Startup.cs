using Owin;

namespace EDelivery.WebPortal
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder context)
        {
            AuthConfig.ConfigureAuth(context);
        }
    }
}

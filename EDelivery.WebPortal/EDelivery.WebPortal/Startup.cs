using Microsoft.Owin;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
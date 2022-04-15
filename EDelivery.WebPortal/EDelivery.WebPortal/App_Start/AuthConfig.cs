using System;
using System.Linq;
using System.Web.Configuration;

using EDelivery.UserStore;
using EDelivery.WebPortal.Authorization;

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.Interop;

using Owin;

namespace EDelivery.WebPortal
{
    public static class AuthConfig
    {
        public const string StripEDeliveryIdentityCookieName = "Strip.EDelivery.Identity";
        public const string EDeliveryIdentityCookieName = "EDelivery.Identity";
        public const string EDeliveryDataProtectionPurpose = "EDelivery.WebPortal";

        public static IDataProtectionProvider DataProtectionProvider { get; private set; }

        public static TimeSpan AuthenticationCookieExpiration { get; private set; }

        public static void ConfigureAuth(IAppBuilder app)
        {
            //create db
            app.CreatePerOwinContext(EDeliveryIdentityDB.Create);

            //create managers
            app.CreatePerOwinContext<EDeliveryUserManager>(EDeliveryUserManager.Create);
            app.CreatePerOwinContext<EDeliverySignInManager>(EDeliverySignInManager.Create);

            string sharedSecretDPKey = WebConfigurationManager.AppSettings["SharedSecretDPKey"];
            if (!string.IsNullOrEmpty(sharedSecretDPKey))
            {
                AuthConfig.DataProtectionProvider = new SharedSecretDataProtectionProvider(sharedSecretDPKey);
            }
            else
            {
                AuthConfig.DataProtectionProvider = new LocalMachineDpapiDataProtectionProvider();
            }

            AuthenticationCookieExpiration =
                TimeSpan.Parse(WebConfigurationManager.AppSettings["AuthenticationCookieExpiration"]);

            app.Use(async (owinContext, next) =>
            {
                await next();

                // or apply UseCookieAuthentication with different options for different routes
                if (owinContext.Environment.ContainsKey(StripEDeliveryIdentityCookieName)
                    && owinContext.Response.Headers.ContainsKey(HeaderNames.SetCookie))
                {
                    owinContext.Response.Headers[HeaderNames.SetCookie] = owinContext.Response.Headers
                        .Where(e => e.Key == HeaderNames.SetCookie)
                        .SelectMany(e => e.Value)
                        .Where(e => !e.Contains(EDeliveryIdentityCookieName))
                        .Aggregate(new StringValues(), (curr, s) => StringValues.Concat(curr, s));
                }
            });

            app.UseCookieAuthentication(new CookieAuthenticationOptions()
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                CookieName = EDeliveryIdentityCookieName,
                CookieSecure = CookieSecureOption.Always,
                CookieHttpOnly = false,
                ExpireTimeSpan = AuthenticationCookieExpiration,
                SlidingExpiration = true,
                LoginPath = new Microsoft.Owin.PathString("/"),
                LogoutPath = new Microsoft.Owin.PathString("/Account/Logout"),
                ReturnUrlParameter = "returnUrl",
                Provider = new CookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user logs in.
                    // This is a security feature which is used when you change a password or add an external login to your account.
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<EDeliveryUserManager, Login, int>(
                        validateInterval: TimeSpan.FromMinutes(10),
                        getUserIdCallback: (id) => id.GetUserId<int>(),
                        regenerateIdentityCallback: (manager, user) => user.GenerateUserIdentityAsync(manager)),
                    OnApplyRedirect = ctx =>
                    {
                        if (!IsAjaxRequest(ctx.Request))
                        {
                            ctx.Response.Redirect(ctx.RedirectUri);
                        }
                    },
                },
                TicketDataFormat =
                    new AspNetTicketDataFormat(
                        AuthConfig.DataProtectionProvider.Create(
                            EDeliveryDataProtectionPurpose))
            });

            app.UseEDeliveryAuthorization();
        }

        private static bool IsAjaxRequest(IOwinRequest request)
        {
            IReadableStringCollection query = request.Query;
            if ((query != null) && (query["X-Requested-With"] == "XMLHttpRequest"))
            {
                return true;
            }

            IHeaderDictionary headers = request.Headers;
            return (headers != null) && (headers["X-Requested-With"] == "XMLHttpRequest");
        }
    }
}

using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;

namespace ED.AdminPanel
{
    public static class HttpContextCultureExtensions
    {
        public static void AppendCultureCookie(
            this HttpContext httpContext,
            CultureInfo culture,
            CultureInfo uiCulture)
        {
            httpContext.AppendCultureCookie(
                new RequestCulture(
                    culture,
                    uiCulture));
        }

        public static void AppendCultureCookie(
            this HttpContext httpContext,
            string culture,
            string uiCulture)
        {
            httpContext.AppendCultureCookie(
                new RequestCulture(
                    culture,
                    uiCulture));
        }

        private static void AppendCultureCookie(
            this HttpContext httpContext,
            RequestCulture reqCulture)
        {
            httpContext.Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(reqCulture),
                new CookieOptions
                {
                    Path = httpContext.Request.PathBase
                });
        }
    }
}

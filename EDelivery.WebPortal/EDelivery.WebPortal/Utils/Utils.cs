using System;
using System.Configuration;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace EDelivery.WebPortal.Utils
{
    public static class Utils
    {
        public static bool MatomoEnabled
        {
            get
            {
                return bool.Parse(ConfigurationManager.AppSettings["MatomoEnabled"]);
            }
        }

        public static string MatomoIdSite
        {
            get
            {
                return ConfigurationManager.AppSettings["MatomoIdSite"];
            }
        }

        public static bool IsProductionEnvironment
        {
            get
            {
                return "ProdV2".Equals(
                    ConfigurationManager.AppSettings["Environment"],
                    StringComparison.InvariantCultureIgnoreCase);
            }
        }

        public static bool IsDevelopmentEnvironment
        {
            get
            {
                return "Development".Equals(
                    ConfigurationManager.AppSettings["Environment"],
                    StringComparison.InvariantCultureIgnoreCase);
            }
        }

        /// <summary>
        /// Get bytes from pdf
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static byte[] GetFileBytes(HttpPostedFileBase file)
        {
            if (file == null)
            {
                return null;
            }
            MemoryStream target = new MemoryStream();
            file.InputStream.CopyTo(target);
            byte[] data = target.ToArray();
            target.Close();
            return data;
        }

        /// <summary>
        /// Get a url to an action
        /// </summary>
        internal static string GetActionUrl(string action, string controller, object routeValues = null)
        {
            if (HttpContext.Current == null || HttpContext.Current.Request == null || HttpContext.Current.Request.RequestContext == null)
            {
                return string.Empty;
            }
            UrlHelper url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            return url.Action(action, controller, routeValues);
        }

        private static readonly string[] UNITS = new string[] { "B", "KB", "MB", "GB", "TB", "PB", "EB" };

        public static string FormatSize(ulong bytes)
        {
            int c = 0;
            for (c = 0; c < UNITS.Length; c++)
            {
                ulong m = (ulong)1 << ((c + 1) * 10);
                if (bytes < m)
                    break;
            }

            double n = bytes / (double)((ulong)1 << (c * 10));
            return string.Format("{0:0.#} {1}", n, UNITS[c]);
        }

        public static string FormatSize(ulong bytes, int unit)
        {
            double n = bytes / (double)((ulong)1 << (unit * 10));
            return string.Format("{0:0.#} {1}", n, UNITS[unit]);
        }

        public static string ToUrlSafeBase64(string message)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(message)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }
    }
}

using System;
using System.Web.Mvc;
using EDelivery.WebPortal.Utils;

using log4net;

namespace EDelivery.WebPortal.Controllers
{
    public class BaseController : Controller
    {
        protected static ILog logger = LogManager.GetLogger("Web.Controller");

        public CachedUserData UserData
        {
            get
            {
                return this.HttpContext.GetCachedUserData();
            }
        }

        /// <summary>
        /// Get regix information
        /// </summary>
        /// <param name="egn"></param>
        /// <returns></returns>
        protected RegixInfoClient.DataContracts.ValidPersonResponse GetRegixPersonInfo(
            string egn)
        {
            logger.Info("Call Regix validpersoninfo with egn:" + egn);

            try
            {
                RegixInfoClient.RegixClient client =
                    new RegixInfoClient.RegixClient();

                RegixInfoClient.DataContracts.ValidPersonResponse response =
                    client.GetValidPersonInfo(egn);
                if (!response.Success)
                {
                    logger.Error($"Response Regix validpersoninfo for egn {egn} returned error {response?.ErrorMessage}");
                }

                return response;
            }
            catch (Exception ex)
            {
                logger.Error($"Error in GetRegixPersonInfo for egn {egn}", ex);
            }

            return null;
        }
    }
}

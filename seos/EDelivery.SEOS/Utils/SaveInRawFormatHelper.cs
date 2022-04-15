using System;
using System.IO;
using System.Text;
using System.Web.Configuration;
using log4net;

namespace EDelivery.SEOS.Utils
{
    public class SaveInRawFormatHelper
    {
        /// <summary>
        /// Save a seos request in file
        /// </summary>
        /// <param name="request"></param>
        /// <param name="dirName"></param>
        /// <param name="fileName"></param>
        public static void Save(string request, string dirName, string fileName, ILog logger)
        {
            try
            {
                if (!Directory.Exists(dirName))
                {
                    Directory.CreateDirectory(dirName);
                }
                string filePath = $"{dirName}\\{fileName}";
                System.IO.File.WriteAllText(filePath, request, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                logger.Error("SaveInRawFormat exception in saving file " + fileName, ex);
            }
        }

        /// <summary>
        /// Save a wrong request in raw format
        /// </summary>
        public static void SaveFaildRequest(string request, int? receiverEntityId, string errorType, ILog logger)
        {
            var name = receiverEntityId.HasValue
                ? receiverEntityId.Value.ToString()
                : String.Empty;

            SaveFaildRequest(request, name, errorType, logger);
        }

        public static void SaveRequest(string request, int? senderEntityId, string errorType, ILog logger)
        {
            var name = senderEntityId.HasValue
                ? senderEntityId.Value.ToString()
                : String.Empty;

            SaveRequest(request, name, errorType, logger);
        }

        public static void SaveFaildRequest(string request, string name, string errorType, ILog logger)
        {
            var folder = WebConfigurationManager.AppSettings["SEOS.FailedRequestsFolder"];
            var fileName = $"{DateTime.Now.ToString("yyyyMMdd hh-mm")} Receiver-{name} {errorType}.xml";
            var dirName = $"{folder}";
            Save(request, dirName, fileName, logger);
        }

        public static void SaveRequest(string request, string name, string errorType, ILog logger)
        {
            var folder = WebConfigurationManager.AppSettings["SEOS.InvalidRequestsFolder"];
            var fileName = $"{DateTime.Now.ToString("yyyyMMdd hh-mm")} Sender-{name} {errorType}.xml";
            var dirName = $"{folder}";
            Save(request, dirName, fileName, logger);
        }
    }
}

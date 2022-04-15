using System;
using EDelivery.SEOS.DatabaseAccess;
using EDelivery.SEOS.DataContracts;
using log4net;

namespace EDelivery.SEOS.Utils
{
    public class LogMessagesHelper
    {
        public static void LogCommunication(
            string messageGuid, 
            MessageType? messageType, 
            string messageBody, 
            bool isRequest, 
            bool isInbound, 
            ILog logger)
        {
            try
            {
                DatabaseQueries.LogCommunication(
                    Guid.Parse(messageGuid),
                    messageType,
                    messageBody,
                    isRequest,
                    isInbound,
                    null);
            }
            catch (Exception ex)
            {
                logger.Error("Can not log communication message: ", ex);
            }
        }

        public static void LogCommunication(
            Guid messageGuid, 
            MessageType? messageType, 
            string messageBody, 
            string endpointUrl, 
            bool isRequest, 
            bool isInbound, 
            ILog logger)
        {
            try
            {
                DatabaseQueries.LogCommunication(
                    messageGuid,
                    messageType,
                    messageBody,
                    isRequest,
                    isInbound,
                    endpointUrl);
            }
            catch (Exception ex)
            {
                logger.Error("Can not log communication message: ", ex);
            }
        }
    }
}

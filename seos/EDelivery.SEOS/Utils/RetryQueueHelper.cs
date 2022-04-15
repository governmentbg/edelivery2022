using System;
using EDelivery.SEOS.DatabaseAccess;
using EDelivery.SEOS.DBEntities;
using log4net;

namespace EDelivery.SEOS.Utils
{
    public class RetryQueueHelper
    {
        /// <summary>
        /// Event to log communication between us and SEOS
        /// </summary>
        /// <param name="messageGuid"></param>
        /// <param name="mSG_DocumentRegistrationRequest"></param>
        /// <param name="seosMessage"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        public static void AddMessage(
            int messageId, 
            int receiverId, 
            string messageBody, 
            ILog logger)
        {
            try
            {
                DatabaseQueries.AddRetrySendRecord(
                    messageId, 
                    receiverId, 
                    messageBody);
            }
            catch (Exception ex)
            {
                logger.Error($"Can not add message in the retry queue " +
                    $"messageId {messageId}, receiverId {receiverId} - error", ex);
            }
        }
    }
}

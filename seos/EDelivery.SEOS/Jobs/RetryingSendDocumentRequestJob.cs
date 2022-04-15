using System;
using System.Collections.Generic;
using System.Linq;
using EDelivery.SEOS.DatabaseAccess;
using EDelivery.SEOS.DataContracts;
using EDelivery.SEOS.DBEntities;
using EDelivery.SEOS.MessagesSend;

namespace EDelivery.SEOS.Jobs
{
    /// <summary>
    /// Job that implements the retry sending message mechannism
    /// </summary>
    public class RetryingSendDocumentRequestJob : Job
    {
        //Общият брой на повторните опити не трябва да надхвърля 10
        private int maxRetryCount = 10;

        public RetryingSendDocumentRequestJob(
            string name)
            : base(name)
        { }

        protected override void Execute()
        {
            try
            {
                var records = DatabaseQueries.GetRetrySendRecords();
                if (records.Count == 0)
                    return;

                logger.Info($"Start retry send, found {records.Count} messages to retry!");

                var result = records
                    .Select(p => (p.Id, TrySendItem(p)))
                    .ToList();

                DatabaseQueries.UpdateRetrySendRecords(result, this.maxRetryCount);
            }
            catch (Exception ex)
            {
                logger.Error("Error in retry send messages: ", ex);
            }
        }

        /// <summary>
        /// Try send an item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private SubmitStatusRequestResult TrySendItem(
            SendMessageProperties properties)
        {
            try
            {
                logger.Info($"TrySendItem message with id {properties.Id}"); 

                var submitHandler = SubmitMessageFactory.CreateInstance(
                    properties.Receiver.Identifier,
                    false,
                    false);

                var result = submitHandler.Submit(properties,
                    MessageType.MSG_DocumentRegistrationRequest,
                    logger);

                return result;
            }
            catch (Exception ex)
            {
                logger.Error("Unknown error in TrySendItem", ex);
                return new SubmitStatusRequestResult()
                {
                    Error = ex.Message,
                    Status = DocumentStatusType.DS_TRY_SEND,
                    StatusResponse = null
                };
            }
        }
    }
}

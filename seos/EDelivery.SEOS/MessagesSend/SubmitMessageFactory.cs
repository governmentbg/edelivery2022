using System;
using EDelivery.SEOS.DatabaseAccess;

namespace EDelivery.SEOS.MessagesSend
{
    public class SubmitMessageFactory
    {
        public static ISubmitMessage CreateInstance(string receiverUic, bool isRetry, bool isThroughEDelivery)
        {
            if (String.IsNullOrEmpty(receiverUic))
                return null;

            if (isThroughEDelivery)
                return new SubmitEDeliveryMessage();

            var as4Node = DatabaseQueries.GetAS4Node(receiverUic);

            if (!String.IsNullOrEmpty(as4Node))
                return new SubmitAs4Message(as4Node);
            else
                return new SubmitSeosMessage(isRetry);
        }
    }
}

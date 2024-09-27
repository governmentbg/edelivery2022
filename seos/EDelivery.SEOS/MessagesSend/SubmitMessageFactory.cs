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

            if (DatabaseQueries.IsAS4Entity(receiverUic))
            {
                var as4Node = DatabaseQueries.GetAS4Node(receiverUic);
                return new SubmitAs4Message(as4Node);
            }

            if (isThroughEDelivery)
                return new SubmitEDeliveryMessage();

            return new SubmitSeosMessage(isRetry);
        }
    }
}

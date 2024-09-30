using EDelivery.SEOS.DatabaseAccess;
using EDelivery.SEOS.Utils;

namespace EDelivery.SEOS.MessagesRedirect
{
    public class RedirectMessageFactory
    {
        public static IRedirectMessage CreateInstance(string uic, string originalSender)
        {
            if (DatabaseQueries.IsAS4Entity(uic))
            {
                var as4Node = DatabaseQueries.GetAS4Node(uic);
                return new RedirectFromAs4ToAs4(as4Node, originalSender);
            }

            if (RegisteredEntitiesHelper.IsThroughEDelivery(uic))
                return new RedirectFromAs4ToEDelivery();

            if (DatabaseQueries.HasOldSeos(uic))
                return new RedirectFromAs4ToSeos();

            return null;
        }
    }
}

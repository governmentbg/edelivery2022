﻿using System;
using EDelivery.SEOS.DatabaseAccess;
using EDelivery.SEOS.Utils;

namespace EDelivery.SEOS.MessagesRedirect
{
    public class RedirectMessageFactory
    {
        public static IRedirectMessage CreateInstance(string uic, string originalSender)
        {
            if (RegisteredEntitiesHelper.IsThroughEDelivery(uic))
                return new RedirectFromAs4ToEDelivery();

            var as4Node = DatabaseQueries.GetAS4Node(uic);
            if (!String.IsNullOrEmpty(as4Node))
                return new RedirectFromAs4ToAs4(as4Node, originalSender);

            if (DatabaseQueries.HasOldSeos(uic))
                return new RedirectFromAs4ToSeos();

            return null;
        }
    }
}

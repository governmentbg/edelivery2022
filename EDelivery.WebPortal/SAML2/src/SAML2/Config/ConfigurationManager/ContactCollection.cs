﻿using System.Configuration;

namespace SAML2.Config.ConfigurationManager
{
    /// <summary>
    /// Service Provider Endpoint configuration collection.
    /// </summary>
    [ConfigurationCollection(typeof(ContactElement), AddItemName = "contact", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class ContactCollection : EnumerableConfigurationElementCollection<ContactElement>
    {
    }
}

﻿using System.Configuration;

namespace SAML2.Config.ConfigurationManager
{
    /// <summary>
    /// Requested Attributes configuration collection.
    /// </summary>
    [ConfigurationCollection(typeof(AttributeElement), CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class RequestedAttributesCollection : EnumerableConfigurationElementCollection<AttributeElement>
    {
    }
}

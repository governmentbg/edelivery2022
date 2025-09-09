﻿using System;
using System.Collections.Generic;

namespace SAML2.Config
{
    /// <summary>
    /// Metadata config item.
    /// </summary>
    public class MetadataConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataConfig"/> class.
        /// </summary>
        public MetadataConfig()
        {
            Contacts = new List<Contact>();
            Lifetime = new TimeSpan(7, 0, 0, 0);
            RequestedAttributes = new List<Attribute>();
        }

        /// <summary>
        /// Gets or sets a value indicating whether to exclude artifact endpoints in metadata generation.
        /// </summary>
        /// <value><c>true</c> if exclude artifact endpoints; otherwise, <c>false</c>.</value>
        public bool ExcludeArtifactEndpoints { get; set; }

        /// <summary>
        /// Gets or sets the lifetime of the metadata. The expiration time is equal to the current time + lifetime.
        /// </summary>
        /// <value>The URL.</value>
        public TimeSpan Lifetime { get; set; }

        /// <summary>
        /// Gets or sets the contacts.
        /// </summary>
        /// <value>The contacts.</value>
        public IList<Contact> Contacts { get; set; }

        /// <summary>
        /// Gets or sets the organization.
        /// </summary>
        /// <value>The organization.</value>
        public Organization Organization { get; set; }

        /// <summary>
        /// Gets or sets the requested attributes.
        /// </summary>
        /// <value>The requested attributes.</value>
        public IList<Attribute> RequestedAttributes { get; set; }
    }
}

using System;

namespace EDelivery.WebPortal.Models.Templates.Components
{
    public abstract class BaseComponent
    {
        public Guid Id { get; set; }

        public string Label { get; set; }

        public ComponentType Type { get; set; }

        public string CustomClass { get; set; }

        public bool IsEncrypted { get; set; }

        public bool IsRequired { get; set; }
    }
}

using System;

namespace ED.Domain
{
    public abstract class BaseComponent
    {
        public Guid Id { get; set; }

        public string Label { get; set; } = null!;

        public ComponentType Type { get; set; }

        public string? CustomClass { get; set; }

        public bool IsEncrypted { get; set; }

        public bool IsRequired { get; set; }
    }
}

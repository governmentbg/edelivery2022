using System;
using System.Text.Json.Serialization;

#pragma warning disable CA1051 // Do not declare visible instance fields
#pragma warning disable CA1708 // Identifiers should differ by more than case

namespace ED.AdminPanel.Blazor.Pages.Templates.Components.Models
{
    public abstract class BaseComponent
    {
        public Guid Id { get; set; }

        public string Label { get; set; }

        public ComponentType Type { get; set; }

        public string CustomClass { get; set; }

        public string DocumentField { get; set; }

        [JsonInclude]
        [JsonPropertyName("IsEncrypted")]
        public string isEncrypted = "false";
        [JsonIgnore]
        public bool IsEncrypted
        {
            get => this.isEncrypted == "true";
            set => this.isEncrypted = value.ToString().ToLowerInvariant();
        }

        [JsonInclude]
        [JsonPropertyName("IsRequired")]
        public string isRequired = "false";
        [JsonIgnore]
        public bool IsRequired
        {
            get => this.isRequired == "true";
            set => this.isRequired = value.ToString().ToLowerInvariant();
        }
    }
}

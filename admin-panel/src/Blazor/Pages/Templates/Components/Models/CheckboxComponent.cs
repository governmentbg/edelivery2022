using System.Text.Json.Serialization;

#pragma warning disable CA1051 // Do not declare visible instance fields
#pragma warning disable CA1708 // Identifiers should differ by more than case

namespace ED.AdminPanel.Blazor.Pages.Templates.Components.Models
{
    public class CheckboxComponent : BaseComponent
    {
        public CheckboxComponent()
        {
            this.Type = ComponentType.checkbox;
        }

        [JsonInclude]
        [JsonPropertyName("Value")]
        public string value = "false";
        [JsonIgnore]
        public bool Value
        {
            get => this.value == "true";
            set => this.value = value.ToString().ToLowerInvariant();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

#nullable enable

namespace ED.AdminPanel.Blazor.Pages.Templates.Components.Models
{
    public static class SerializationHelper
    {
        private static JsonSerializerOptions jsonSerializerOptions =
            new() { Converters = { new JsonStringEnumConverter() } };
        
        public static string SerializeModel(IList<BaseComponent> model)
        {
            return JsonSerializer.Serialize(
                // use Cast<object> as the model is a polymorphyc list
                // and we want it to use the actual type of each item
                model.Cast<object>(),
                jsonSerializerOptions);
        }

        public static IList<BaseComponent> DeserializeModel(string json)
        {
            using JsonDocument doc = JsonDocument.Parse(json);

            if (doc.RootElement.ValueKind != JsonValueKind.Array)
            {
                throw new Exception("A json array is expected");
            }

            List<BaseComponent> result = new();

            foreach (JsonElement item in doc.RootElement.EnumerateArray())
            {
                var type =
                    item.GetProperty("Type").GetString()
                    ?? throw new Exception("Missing required property Type");

                ComponentType typeEnum =
                    (ComponentType)Enum.Parse(typeof(ComponentType), type);

                switch (typeEnum)
                {
                    case ComponentType.hidden:
                        var hiddenComponent =
                            item.ToObject<HiddenComponent>(jsonSerializerOptions)
                            ?? throw new Exception($"The model should be serializable to {nameof(HiddenComponent)}");
                        result.Add(hiddenComponent);
                        break;

                    case ComponentType.textfield:
                        var textFieldComponent =
                            item.ToObject<TextFieldComponent>(jsonSerializerOptions)
                            ?? throw new Exception($"The model should be serializable to {nameof(TextFieldComponent)}");
                        result.Add(textFieldComponent);
                        break;

                    case ComponentType.textarea:
                        var textAreaComponent =
                            item.ToObject<TextAreaComponent>(jsonSerializerOptions)
                            ?? throw new Exception($"The model should be serializable to {nameof(TextAreaComponent)}");
                        result.Add(textAreaComponent);
                        break;

                    case ComponentType.datetime:
                        var datetimeComponent =
                            item.ToObject<DateTimeComponent>(jsonSerializerOptions)
                            ?? throw new Exception($"The model should be serializable to {nameof(DateTimeComponent)}");
                        result.Add(datetimeComponent);
                        break;

                    case ComponentType.select:
                        var selectComponent =
                            item.ToObject<SelectComponent>(jsonSerializerOptions)
                            ?? throw new Exception($"The model should be serializable to {nameof(SelectComponent)}");
                        result.Add(selectComponent);
                        break;

                    case ComponentType.checkbox:
                        var checkboxComponent =
                            item.ToObject<CheckboxComponent>(jsonSerializerOptions)
                            ?? throw new Exception($"The model should be serializable to {nameof(CheckboxComponent)}");
                        result.Add(checkboxComponent);
                        break;

                    case ComponentType.file:
                        var fileComponent =
                            item.ToObject<FileComponent>(jsonSerializerOptions)
                            ?? throw new Exception($"The model should be serializable to {nameof(FileComponent)}");
                        result.Add(fileComponent);
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }

            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace ED.Domain
{
    public static class TemplateSerializationHelper
    {
        public static IList<BaseComponent> DeserializeModel(string json)
        {
            JArray arr = JArray.Parse(json);

            List<BaseComponent> result = new();

            foreach (var item in arr.Children<JObject>())
            {
                string type = item.Properties().SingleOrDefault(e => e.Name == "Type")?.Value.Value<string>()
                    ?? throw new Exception("Missing required property Type");

                ComponentType typeEnum =
                    (ComponentType)Enum.Parse(typeof(ComponentType), type);

                switch (typeEnum)
                {
                    case ComponentType.hidden:
                        HiddenComponent hiddenComponent =
                            item.ToObject<HiddenComponent>()
                                ?? throw new Exception($"The model should be serializable to {nameof(HiddenComponent)}");
                        result.Add(hiddenComponent);
                        break;

                    case ComponentType.textfield:
                        TextFieldComponent textFieldComponent =
                            item.ToObject<TextFieldComponent>()
                                ?? throw new Exception($"The model should be serializable to {nameof(TextFieldComponent)}");
                        result.Add(textFieldComponent);
                        break;

                    case ComponentType.textarea:
                        TextAreaComponent textAreaComponent =
                            item.ToObject<TextAreaComponent>()
                                ?? throw new Exception($"The model should be serializable to {nameof(TextAreaComponent)}");
                        result.Add(textAreaComponent);
                        break;

                    case ComponentType.datetime:
                        DateTimeComponent datetimeComponent =
                            item.ToObject<DateTimeComponent>()
                                ?? throw new Exception($"The model should be serializable to {nameof(DateTimeComponent)}");
                        result.Add(datetimeComponent);
                        break;

                    case ComponentType.select:
                        SelectComponent selectComponent =
                            item.ToObject<SelectComponent>()
                                ?? throw new Exception($"The model should be serializable to {nameof(SelectComponent)}");
                        result.Add(selectComponent);
                        break;

                    case ComponentType.checkbox:
                        CheckboxComponent checkboxComponent =
                            item.ToObject<CheckboxComponent>()
                                ?? throw new Exception($"The model should be serializable to {nameof(CheckboxComponent)}");
                        result.Add(checkboxComponent);
                        break;

                    case ComponentType.file:
                        FileComponent fileComponent =
                            item.ToObject<FileComponent>()
                                ?? throw new Exception($"The model should be serializable to {nameof(FileComponent)}");
                        result.Add(fileComponent);
                        break;

                    case ComponentType.markdown:
                        MarkdownComponent labelComponent =
                            item.ToObject<MarkdownComponent>()
                                ?? throw new Exception($"The model should be serializable to {nameof(MarkdownComponent)}");
                        result.Add(labelComponent);
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }

            return result;
        }
    }
}

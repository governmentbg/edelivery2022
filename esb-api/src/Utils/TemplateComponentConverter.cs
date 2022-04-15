using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ED.EsbApi;

public class TemplateComponentConverter : JsonConverter
{
    public override object? ReadJson(
        JsonReader reader,
        Type objectType,
        object? existingValue,
        JsonSerializer serializer)
    {
        JArray ja = JArray.Load(reader);

        List<BaseComponent> result = new(ja.Count);

        foreach (JObject item in ja)
        {
            string type = item["Type"]!.Value<string>()!;
            ComponentType componentType = Enum.Parse<ComponentType>(type);

            BaseComponent? component = null;

            Dictionary<string, string?> dict =
                item.ToObject<Dictionary<string, string?>>()!;

            switch (componentType)
            {
                case ComponentType.textarea:
                    component = new TextAreaComponent(
                        Guid.Parse(dict["Id"]!),
                        dict["Label"]!,
                        bool.Parse(dict["IsEncrypted"]!),
                        bool.Parse(dict["IsRequired"]!),
                        dict["Value"]);
                    break;
                case ComponentType.hidden:
                    component = new HiddenComponent(
                        Guid.Parse(dict["Id"]!),
                        dict["Label"]!,
                        bool.Parse(dict["IsEncrypted"]!),
                        bool.Parse(dict["IsRequired"]!),
                        dict["Value"]);
                    break;
                case ComponentType.textfield:
                    component = new TextFieldComponent(
                        Guid.Parse(dict["Id"]!),
                        dict["Label"]!,
                        bool.Parse(dict["IsEncrypted"]!),
                        bool.Parse(dict["IsRequired"]!),
                        dict["Value"]);
                    break;
                case ComponentType.datetime:
                    component = new DateTimeComponent(
                        Guid.Parse(dict["Id"]!),
                        dict["Label"]!,
                        bool.Parse(dict["IsEncrypted"]!),
                        bool.Parse(dict["IsRequired"]!),
                        dict["Value"]);
                    break;
                case ComponentType.checkbox:
                    component = new CheckboxComponent(
                        Guid.Parse(dict["Id"]!),
                        dict["Label"]!,
                        bool.Parse(dict["IsEncrypted"]!),
                        bool.Parse(dict["IsRequired"]!),
                        bool.TryParse(dict["Value"], out bool bValue) && bValue);
                    break;
                case ComponentType.select:
                    component = new SelectComponent(
                        Guid.Parse(dict["Id"]!),
                        dict["Label"]!,
                        bool.Parse(dict["IsEncrypted"]!),
                        bool.Parse(dict["IsRequired"]!),
                        dict["Value"],
                        dict["Url"],
                        dict["Options"]?.Split(",", StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>());
                    break;
                case ComponentType.file:
                    component = new FileComponent(
                        Guid.Parse(dict["Id"]!),
                        dict["Label"]!,
                        bool.Parse(dict["IsEncrypted"]!),
                        bool.Parse(dict["IsRequired"]!),
                        int.Parse(dict["MaxSize"]!),
                        dict["AllowedExtensions"]!,
                        int.Parse(dict["Instances"]!));
                    break;
                default:
                    throw new Exception("Unsupported template field");
            }

            result.Add(component);
        }

        return result;
    }

    public override bool CanWrite => false;

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override bool CanConvert(Type objectType)
    {
        return typeof(List<BaseComponent>).IsAssignableFrom(objectType);
    }
}

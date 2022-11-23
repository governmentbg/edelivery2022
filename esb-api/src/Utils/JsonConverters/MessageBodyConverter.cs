using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ED.EsbApi;

public class FileObject
{
    public int? FileId { get; set; }

    public string FileName { get; set; } = null!;

    public string FileHash { get; set; } = null!;
}

public class MessageBodyConverter : JsonConverter
{
    private IList<BaseComponent> components;

    public MessageBodyConverter(IList<BaseComponent> components)
    {
        this.components = components;
    }

    public override object? ReadJson(
        JsonReader reader,
        Type objectType,
        object? existingValue,
        JsonSerializer serializer)
    {
        JObject obj = JObject.Load(reader);

        Dictionary<Guid, object?> result = new();

        foreach (KeyValuePair<string, JToken?> prop in obj)
        {
            Guid id = Guid.Parse(prop.Key);
            object? value = null;

            BaseComponent component = this.components.First(e => e.Id == id);

            switch (component.Type)
            {
                case ComponentType.hidden:
                case ComponentType.textfield:
                case ComponentType.textarea:
                case ComponentType.datetime:
                case ComponentType.select:
                case ComponentType.markdown:
                    value = prop.Value?.Value<string>() ?? string.Empty;
                    break;
                case ComponentType.checkbox:
                    value = prop.Value?.Value<bool>() ?? false;
                    break;
                case ComponentType.file:
                    if (prop.Value is JArray arr)
                    {
                        value = arr.ToObject<FileObject[]>();
                    }
                    else
                    {
                        value = Array.Empty<FileObject>();
                    }
                    break;
                default:
                    throw new Exception("Unsupported template field");
            }

            result.Add(id, value);
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
        return typeof(Dictionary<Guid, object?>).IsAssignableFrom(objectType);
    }
}

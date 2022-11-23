using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Newtonsoft.Json.Linq;

namespace ED.EsbApi;

public sealed class SystemTemplateUtils
{
    private static readonly string[] UNITS = new string[] { "B", "KB", "MB", "GB", "TB", "PB", "EB" };

    public static string FormatSize(ulong bytes)
    {
        int c = 0;
        for (c = 0; c < UNITS.Length; c++)
        {
            ulong m = (ulong)1 << ((c + 1) * 10);
            if (bytes < m)
                break;
        }

        double n = bytes / (double)((ulong)1 << (c * 10));
        return string.Format("{0:0.#} {1}", n, UNITS[c]);
    }

    private static readonly JsonSerializerOptions JsonSerializerOptions =
        new()
        {
            Encoder = JavaScriptEncoder.Create(
                UnicodeRanges.BasicLatin,
                UnicodeRanges.Cyrillic)
        };

    public static (string, string) GetNewMessageBodyJson(
        IList<BaseComponent> components,
        Dictionary<Guid, object?> fields,
        DomainServices.Esb.GetBlobsInfoResponse.Types.Blob[] blobs)
    {
        Dictionary<string, object?> body = new();
        Dictionary<string, object?> metaFields = new();

        foreach (KeyValuePair<Guid, object?> item in fields)
        {
            BaseComponent matchingComponent = components.First(e => e.Id == item.Key);

            object? value = null;

            switch (matchingComponent.Type)
            {
                case ComponentType.hidden:
                case ComponentType.textfield:
                case ComponentType.textarea:
                case ComponentType.datetime:
                case ComponentType.select:
                    value = item.Value ?? string.Empty;
                    break;
                case ComponentType.checkbox:
                    value = item.Value ?? false;
                    break;
                case ComponentType.file:
                    if (item.Value == null)
                    {
                        value = null;
                    }
                    else
                    {
                        value = ((JArray)item.Value!).ToObject<int[]>()!.Select(e =>
                        {
                            var machingBlob = blobs.First(b => b.BlobId == e);

                            return new
                            {
                                FileId = machingBlob.BlobId,
                                FileName = $"{machingBlob.FileName} ({FormatSize((ulong)machingBlob.Size!)})",
                                FileHash = $"{machingBlob.HashAlgorithm}: {machingBlob.Hash}"
                            };
                        });
                    }
                    break;
                case ComponentType.markdown:
                    break;
                default:
                    throw new Exception("Unsupported template field");
            }

            body.Add(item.Key.ToString(), value);

            if (!matchingComponent.IsEncrypted)
            {
                metaFields.Add(item.Key.ToString(), value);
            }
        }

        string bodyAsString = JsonSerializer.Serialize(body, JsonSerializerOptions);
        string metaFieldsAsString = JsonSerializer.Serialize(metaFields, JsonSerializerOptions);

        return (bodyAsString, metaFieldsAsString);
    }
}

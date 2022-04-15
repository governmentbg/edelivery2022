using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

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

    public record BlobInfo(
        Guid FieldId,
        string FileName,
        string Alg,
        string Hash,
        ulong Bytes);

    public static string GetNewMessageBodyJson(
       string body,
       BlobInfo[] files)
       => JsonSerializer.Serialize(
            new Dictionary<string, object>
            {
                {
                    "179ea4dc-7879-43ad-8073-72b263915656",
                    body
                },
                {
                    "e2135802-5e34-4c60-b36e-c86d910a571a",
                    files.Select(f =>
                        new
                        {
                            FileName = $"{f.FileName} ({FormatSize(f.Bytes)})",
                            FileHash = $"{f.Alg}: {f.Hash}"
                        })
                        .ToArray()
                }
            },
            JsonSerializerOptions);

    public static (string, string) GetNewMessageBodyJson(
#pragma warning disable CA1002 // Do not expose generic lists
        List<BaseComponent> components,
#pragma warning restore CA1002 // Do not expose generic lists
        Dictionary<Guid, string?> fields,
        BlobInfo[] files)
    {
        Dictionary<string, object> body = new();
        Dictionary<string, object> metaFields = new();

        foreach (var item in fields)
        {
            body.Add(item.Key.ToString(), item.Value ?? string.Empty);

            if (!components.First(e => e.Id == item.Key).IsEncrypted)
            {
                metaFields.Add(item.Key.ToString(), item.Value ?? string.Empty);
            }
        }

        foreach (var fieldFiles in files.GroupBy(e => e.FieldId))
        {
            var value = fieldFiles.Select(e => new
            {
                FileName = $"{e.FileName} ({FormatSize(e.Bytes)})",
                FileHash = $"{e.Alg}: {e.Hash}"
            });

            body.Add(fieldFiles.Key.ToString(), value);

            if (!components.First(e => e.Id == fieldFiles.Key).IsEncrypted)
            {
                metaFields.Add(fieldFiles.Key.ToString(), value);
            }
        }

        string bodyAsString = JsonSerializer.Serialize(body, JsonSerializerOptions);
        string metaFieldsAsString = JsonSerializer.Serialize(metaFields, JsonSerializerOptions);

        return (bodyAsString, metaFieldsAsString);
    }
}

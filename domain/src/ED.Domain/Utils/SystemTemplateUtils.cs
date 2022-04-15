using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace ED.Domain
{
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

        public static string GetNewMessageBodyJson(
           string body,
           (string fileName, string alg, string hash, ulong bytes)[] files)
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
                                    FileName = $"{f.fileName} ({FormatSize(f.bytes)})",
                                    FileHash = $"{f.alg}: {f.hash}"
                                })
                                .ToArray()
                        }
                   },
                   JsonSerializerOptions);
    }
}

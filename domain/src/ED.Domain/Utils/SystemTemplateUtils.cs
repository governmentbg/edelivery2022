using System;
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
#pragma warning disable IDE0059 // Unnecessary assignment of a value
            int c = 0;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
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

        private static readonly JsonSerializerOptions IndentedJsonSerializerOptions =
            new()
            {
                Encoder = JavaScriptEncoder.Create(
                    UnicodeRanges.BasicLatin,
                    UnicodeRanges.Cyrillic),
                WriteIndented = true,
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

        public static string GetTicketMessageBody(
            string body,
            string type,
            string? documentSeries,
            string documentNumber,
            DateTime issueDate,
            string vehicleNumber,
            DateTime violationDate,
            string violatedProvision,
            string penaltyProvision,
            string dueAmount,
            string discountedPaymentAmount,
            string iban,
            string bic,
            string paymentReason,
            (string fileName, string alg, string hash, ulong bytes) document)
        {
            Dictionary<string, object?> dict = new()
            {
                {
                    "d7f6dcc0-7dbd-4e8f-b600-8dbfb6a05a49",
                    body
                },
                {
                    "93803a3b-3040-4d4c-a47c-c2c85dcbacc6",
                    type
                },
                {
                    "a1ad8553-8d2c-4dcb-aee7-8d3dd7414cfe",
                    documentSeries
                },
                {
                    "0a66e4d7-0d11-4d8f-bf4a-40c64035b25f",
                    documentNumber
                },
                {
                    "dc0d7323-33fd-47af-84f8-361e8a4f94a0",
                    issueDate
                },
                {
                    "fb85be80-ce2b-45b0-baa3-2e8605d756bf",
                    vehicleNumber
                },
                {
                    "ef1fcb02-2727-4826-bff5-3f7fa47d1e6b",
                    violationDate
                },
                {
                    "c036dd96-6b63-45d7-bade-13d17becdd47",
                    violatedProvision
                },
                {
                    "8ade1621-8541-4a3d-bdaa-b75111838685",
                    penaltyProvision
                },
                {
                    "5b6a0a84-9ad9-4c63-8a68-e6b0391d5859",
                    dueAmount
                },
                {
                    "9ad76167-b99e-4399-ab7d-a91718c1a325",
                    discountedPaymentAmount
                },
                {
                    "507a4410-fa66-4b4d-a0ba-7e3a0bd2b160",
                    iban
                },
                {
                    "e32d48ad-486b-4e0d-bacf-5123afaf5648",
                    bic
                },
                {
                    "0f56411f-b626-4e05-aefe-711401a6a31a",
                    paymentReason
                },
                {
                    "87ea1787-2fa7-49ad-b384-59e68f59a1b7",
                    new[]
                    {
                        new
                        {
                            FileName = $"{document.fileName} ({FormatSize(document.bytes)})",
                            FileHash = $"{document.alg}: {document.hash}"
                        }
                    }
                },
            };

            string json =
                JsonSerializer.Serialize(dict, IndentedJsonSerializerOptions);

            return json;
        }
    }
}

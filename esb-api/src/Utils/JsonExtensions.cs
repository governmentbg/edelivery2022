using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

namespace ED.EsbApi;

public static class JsonExtensions
{
    public static Dictionary<Guid, string?> ParseMessageBody(string body)
    {
        Type stringType = typeof(string);

        return JsonConvert.DeserializeObject<Dictionary<Guid, object>>(body)?
            .Where(e => e.Value.GetType().Equals(stringType)
                || e.Value == null)
            .ToDictionary(k => k.Key, v => Convert.ToString(v.Value))
                ?? new Dictionary<Guid, string?>();
    }
}

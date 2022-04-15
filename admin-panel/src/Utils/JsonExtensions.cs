using System;
using System.Buffers;
using System.Text.Json;

#nullable enable

namespace ED.AdminPanel
{
    // copied from https://stackoverflow.com/questions/58138793/system-text-json-jsonelement-toobject-workaround

    // should be replaced with native methods in dotnet 6
    // see https://github.com/dotnet/runtime/issues/31274#issuecomment-883578067

    public static class JsonExtensions
    {
        public static T? ToObject<T>(this JsonElement element, JsonSerializerOptions? options = null)
        {
            var bufferWriter = new ArrayBufferWriter<byte>();
            using (var writer = new Utf8JsonWriter(bufferWriter))
                element.WriteTo(writer);
            return JsonSerializer.Deserialize<T>(bufferWriter.WrittenSpan, options);
        }

        public static T? ToObject<T>(this JsonDocument document, JsonSerializerOptions? options = null)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));
            return document.RootElement.ToObject<T>(options);
        }
    }
}

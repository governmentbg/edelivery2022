using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ED.Blobs
{
    // copied from
    // https://github.com/Macross-Software/core/blob/develop/ClassLibraries/Macross.Json.Extensions/Code/System.Text.Json.Serialization/JsonTimeSpanConverter.cs

    /// <summary>
	/// <see cref="JsonConverterFactory"/> to convert <see cref="TimeSpan"/>
    /// to and from strings. Supports <see cref="Nullable{TimeSpan}"/>.
	/// </summary>
	/// <remarks>
	/// TimeSpans are transposed using the constant ("c") format specifier:
    /// [-][d.]hh:mm:ss[.fffffff]
	/// </remarks>
	public class JsonTimeSpanConverter : JsonConverterFactory
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type typeToConvert)
        {
            // Don't perform a typeToConvert == null check for performance.
            // Trust our callers will be nice.
#pragma warning disable CA1062 // Validate arguments of public methods
            return typeToConvert == typeof(TimeSpan)
                || (typeToConvert.IsGenericType && IsNullableTimeSpan(typeToConvert));
#pragma warning restore CA1062 // Validate arguments of public methods
        }

        /// <inheritdoc/>
        public override JsonConverter CreateConverter(
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            // Don't perform a typeToConvert == null check for performance.
            // Trust our callers will be nice.
#pragma warning disable CA1062 // Validate arguments of public methods
            return typeToConvert.IsGenericType
                ? new JsonNullableTimeSpanConverter()
                : new JsonStandardTimeSpanConverter();
#pragma warning restore CA1062 // Validate arguments of public methods
        }

        private static bool IsNullableTimeSpan(Type typeToConvert)
        {
            Type? UnderlyingType = Nullable.GetUnderlyingType(typeToConvert);

            return UnderlyingType != null && UnderlyingType == typeof(TimeSpan);
        }

        private class JsonStandardTimeSpanConverter : JsonConverter<TimeSpan>
        {
            /// <inheritdoc/>
            public override TimeSpan Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.String)
                    throw new Exception($"Unable to convert value to {nameof(TimeSpan)}");

                string value = reader.GetString()!;
                try
                {
                    return TimeSpan.ParseExact(value, "c", CultureInfo.InvariantCulture);
                }
                catch (Exception parseException)
                {
                    throw new Exception($"Unable to convert value {value} to {nameof(TimeSpan)}", parseException);
                }
            }

            /// <inheritdoc/>
            public override void Write(
                Utf8JsonWriter writer,
                TimeSpan value,
                JsonSerializerOptions options)
                => writer.WriteStringValue(value.ToString("c", CultureInfo.InvariantCulture));
        }

        private class JsonNullableTimeSpanConverter : JsonConverter<TimeSpan?>
        {
            /// <inheritdoc/>
            public override TimeSpan? Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.String)
                    throw new Exception($"Unable to convert value to nullable {nameof(TimeSpan)}");

                string value = reader.GetString()!;
                try
                {
                    return TimeSpan.ParseExact(
                        value,
                        "c",
                        CultureInfo.InvariantCulture);
                }
                catch (Exception parseException)
                {
                    throw new Exception($"Unable to convert value {value} to nullable {nameof(TimeSpan)}", parseException);
                }
            }

            /// <inheritdoc/>
            public override void Write(
                Utf8JsonWriter writer,
                TimeSpan? value,
                JsonSerializerOptions options)
                => writer.WriteStringValue(
                    value!.Value.ToString("c", CultureInfo.InvariantCulture));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Keycloak.Net.Common.Converters
{
    /// <summary>
    /// Provides an extension for <see cref="JsonStringEnumConverter"/> that adds support for <see cref="EnumMemberAttribute"/>.
    /// </summary>
    /// <typeparam name="TEnum">The type of the <see cref="TEnum"/>.</typeparam>
    public sealed class EnumMemberJsonStringEnumConverter<TEnum> : JsonConverter<TEnum>
        where TEnum : struct, Enum
    {
        private static readonly Dictionary<string, string> _enumMemberMap;
        private static readonly Dictionary<string, TEnum> _valueMap;

        static EnumMemberJsonStringEnumConverter()
        {
            _enumMemberMap = typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static)
                .Select(f => (f.Name, AttributeName: f.GetCustomAttribute<EnumMemberAttribute>()?.Value))
                .Where(pair => pair.AttributeName != null)
                .ToDictionary(pair => pair.Name, pair => pair.AttributeName!);

            _valueMap = _enumMemberMap.ToDictionary(pair => pair.Value, pair => Enum.Parse<TEnum>(pair.Key));
        }

        public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException("Expected string token.");
            }

            string enumString = reader.GetString()!;
            if (_valueMap.TryGetValue(enumString, out var value))
            {
                return value;
            }

            throw new JsonException($"Unable to parse \"{enumString}\" to Enum \"{typeof(TEnum).Name}\".");
        }

        public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
        {
            string name = value.ToString();
            if (_enumMemberMap.TryGetValue(name, out var enumMemberName))
            {
                writer.WriteStringValue(enumMemberName);
            }
            else
            {
                writer.WriteStringValue(name);
            }
        }
    }
}

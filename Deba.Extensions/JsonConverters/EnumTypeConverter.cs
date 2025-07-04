using System.Text.Json;
using System.Text.Json.Serialization;

namespace Deba.Extensions.JsonConverters;

public class EnumTypeConverter<TEnum> : JsonConverter<TEnum> where TEnum : struct, Enum
{
    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                var stringValue = reader.GetString();

                return Enum.GetValues(typeof(TEnum))
                    .OfType<TEnum>()
                    .ToList()
                    .FirstOrDefault(x => x.GetDescription().Equals(stringValue));

            case JsonTokenType.Number:
                var numValue = reader.GetInt32();

                if (Enum.IsDefined(typeof(TEnum), numValue))
                    return (TEnum)Enum.ToObject(typeof(TEnum), numValue);

                return default;

            default:
                return default;
        }
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.GetDescription());
    }
}
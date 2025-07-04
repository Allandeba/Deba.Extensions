using System.Text.Json;
using System.Text.Json.Serialization;

namespace Deba.Extensions.JsonConverters;

/*
    builder.Services.Configure<JsonOptions>(opt =>
    {
        opt.SerializerOptions.Converters.Add(new EnumDescriptionConverterFactory());
    });
*/
public class EnumDescriptionConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsEnum;
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(EnumTypeConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}
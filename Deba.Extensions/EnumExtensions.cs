using System.ComponentModel;

namespace Deba.Extensions;

public static class EnumExtensions
{
    public static int GetValue<TEnum>(this TEnum enumType) where TEnum : struct, Enum =>
        Convert.ToInt32(enumType);

    public static string GetDescription(this Enum enumValue)
    {
        var enumAsString = enumValue.ToString();

        var field = enumValue.GetType().GetField(enumAsString);
        if (field is null) return enumAsString;

        var attributes = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
        return attributes is null ? enumAsString : attributes.Description;
    }

    public static string[] GetAllDescriptions<TEnum>() where TEnum : struct, Enum =>
        Enum.GetValues<TEnum>()
            .Select(enumValue => enumValue.GetDescription())
            .ToArray();

    public static TEnum GetValue<TEnum>(string enumDescription) where TEnum : struct, Enum
    {
        var type = typeof(TEnum);
        foreach (var field in type.GetFields())
        {
            if (!field.IsStatic)
                continue;

            var descriptionAttribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;

            var description = descriptionAttribute is not null
                ? descriptionAttribute.Description
                : field.Name;

            if (string.Equals(description, enumDescription, StringComparison.InvariantCultureIgnoreCase)
                || string.Equals(field.Name, enumDescription, StringComparison.InvariantCultureIgnoreCase))
            {
                return (TEnum)field.GetValue(null)!;
            }
        }

        throw new ArgumentException($"Nenhum valor do enum {type.Name} com descrição '{enumDescription}' foi encontrado.", nameof(enumDescription));
    }
    
    public static bool IsEqualTo<TEnum>(this string enumDescription, TEnum enumValue) where TEnum : struct, Enum
    {
        var parsedValue = GetValue<TEnum>(enumDescription);
        return EqualityComparer<TEnum>.Default.Equals(parsedValue, enumValue);
    }
}
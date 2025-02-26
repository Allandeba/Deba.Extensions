using System.ComponentModel;

namespace Deba.Extensions;

public static class EnumExtensions
{
    public static int GetValue<T>(this T enumType) where T : Enum =>
        Convert.ToInt32(enumType);

    public static string GetDescription(this Enum enumValue)
    {
        var enumAsString = enumValue.ToString();
        
        var field = enumValue.GetType().GetField(enumAsString);
        if (field is null) return enumAsString;
        
        var attributes = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
        return attributes is null ? enumAsString : attributes.Description;
    }
    
}
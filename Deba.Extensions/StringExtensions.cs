using System.Text;
using System.Text.RegularExpressions;

namespace Deba.Extensions;

public static class StringExtensions
{
    public static string Unaccent(this string content)
    {
        var normalized = Regex.Replace(
            content.Normalize(NormalizationForm.FormD),
            @"[\p{Mn}]",
            ""
        );
        return normalized.Normalize(NormalizationForm.FormC);
    }

    public static string OnlyNumbers(this string content) =>
        Regex.Replace(content, @"[^\d]", "");

    public static string OnlyAlphanumeric(this string content) =>
        Regex.Replace(content, @"[^a-zA-Z0-9]", "");
}
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Deba.Extensions.JsonConverters;

namespace Deba.Extensions;

public static class HttpClientExtensions
{
    private static JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new EnumDescriptionConverterFactory() },
    };

    public static Task<T?> ReadFromJsonWithOptionsAsync<T>(this HttpContent content, CancellationToken cancellationToken = default)
    {
        return content.ReadFromJsonAsync<T>(_jsonOptions, cancellationToken);
    }

    public static Task<HttpResponseMessage> PostAsJsonWithOptionsAsync<T>(this HttpClient client, string requestUri, T value)
    {
        return client.PostAsJsonAsync(requestUri, value, _jsonOptions);
    }
}
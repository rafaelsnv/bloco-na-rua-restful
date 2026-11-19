using System.Text.Json;

namespace BlocoNaRua.Tests.Helpers;

/// <summary>
/// HttpClient helpers for x-www-form-urlencoded POSTs (login/signup form endpoints).
/// </summary>
public static class HttpClientFormExtensions
{
    /// <summary>
    /// POSTs <paramref name="fields"/> as application/x-www-form-urlencoded content.
    /// </summary>
    /// <param name="client">The HTTP client used to send the request.</param>
    /// <param name="requestUri">The request URI.</param>
    /// <param name="fields">The form fields to submit.</param>
    /// <returns>The response returned by the API.</returns>
    public static Task<HttpResponseMessage> PostAsFormUrlEncodedAsync(
        this HttpClient client,
        string requestUri,
        Dictionary<string, string> fields)
    {
        var content = new FormUrlEncodedContent(fields);
        return client.PostAsync(requestUri, content);
    }

    /// <summary>
    /// POSTs an anonymous object's properties as application/x-www-form-urlencoded content.
    /// Null values become empty strings, strings are preserved, and other values are JSON-serialized.
    /// </summary>
    /// <param name="client">The HTTP client used to send the request.</param>
    /// <param name="requestUri">The request URI.</param>
    /// <param name="payload">The object whose public properties become form fields.</param>
    /// <returns>The response returned by the API.</returns>
    public static Task<HttpResponseMessage> PostAsFormUrlEncodedAsync(
        this HttpClient client,
        string requestUri,
        object payload)
    {
        var fields = new Dictionary<string, string>();
        foreach (var property in payload.GetType().GetProperties())
        {
            var value = property.GetValue(payload);
            fields[property.Name] = value switch
            {
                null => string.Empty,
                string s => s,
                _ => JsonSerializer.Serialize(value),
            };
        }

        return client.PostAsFormUrlEncodedAsync(requestUri, fields);
    }
}

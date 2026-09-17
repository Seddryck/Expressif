using System.Net.Http.Headers;
using System.Text;

namespace Expressif.OpenLineage;

/// <summary>
/// Sends events to an OpenLineage HTTP endpoint using a caller-owned HTTP client.
/// </summary>
public sealed class HttpOpenLineageTransport : IOpenLineageTransport
{
    private readonly HttpClient client;
    private readonly Uri endpoint;
    private readonly string? apiKey;

    public HttpOpenLineageTransport(HttpClient client, Uri url, string endpoint = "api/v1/lineage", string? apiKey = null)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(url);
        if (!url.IsAbsoluteUri || url.Scheme is not ("http" or "https"))
            throw new ArgumentException("The OpenLineage URL must be an absolute HTTP or HTTPS URL.", nameof(url));
        if (Uri.TryCreate(endpoint.TrimStart('/'), UriKind.Absolute, out _) || endpoint.StartsWith("//", StringComparison.Ordinal))
            throw new ArgumentException("The endpoint must be a relative path.", nameof(endpoint));

        this.client = client;
        this.endpoint = new Uri(new Uri(url.AbsoluteUri.TrimEnd('/') + "/"), endpoint.TrimStart('/'));
        this.apiKey = apiKey;
    }

    public void Emit(string json)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        if (!string.IsNullOrWhiteSpace(apiKey))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        using var response = client.SendAsync(request).GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();
    }
}

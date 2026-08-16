using System.Net.Http.Json;

namespace CoordiNet.Tunnels;

public static class LinkShortenerClient
{
    private static readonly HttpClient Http = new();

    public static async Task<string> ShortenUrlAsync(string originalUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(originalUrl))
        {
            return string.Empty;
        }

        var trimmed = originalUrl.Trim();
        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out _))
        {
            return trimmed;
        }

        var endpoint = Environment.GetEnvironmentVariable("COORDINET_SHORTENER_ENDPOINT")
            ?? Environment.GetEnvironmentVariable("SHORTENER_ENDPOINT");

        if (string.IsNullOrWhiteSpace(endpoint))
        {
            return trimmed;
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = JsonContent.Create(new { url = trimmed });

            using var response = await Http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return trimmed;
            }

            var payload = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>(cancellationToken: cancellationToken);
            if (payload is not null)
            {
                foreach (var key in new[] { "shortUrl", "shortenedUrl", "url", "link", "result" })
                {
                    if (payload.TryGetValue(key, out var candidate) && !string.IsNullOrWhiteSpace(candidate))
                    {
                        return candidate;
                    }
                }
            }

            var text = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(text) && Uri.TryCreate(text.Trim(), UriKind.Absolute, out var parsed))
            {
                return parsed.ToString();
            }
        }
        catch
        {
            return trimmed;
        }

        return trimmed;
    }
}

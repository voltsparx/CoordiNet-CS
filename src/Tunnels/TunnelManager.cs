namespace CoordiNet.Tunnels;

public static class TunnelManager
{
    public static async Task<TunnelSession?> StartAsync(string providerName, int localPort)
    {
        if (string.Equals(providerName, "none", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        ITunnelProvider provider = providerName.ToLowerInvariant() switch
        {
            "ngrok" => new NgrokProvider(),
            "cloudflared" or "cf" => new CloudflareProvider(),
            _ => throw new ArgumentException($"Unsupported tunnel provider: {providerName}")
        };

        var url = await provider.StartAsync(localPort);

        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        var deploymentUrl = RuntimeOptions.EnableShortening
            ? await LinkShortenerClient.ShortenUrlAsync(url)
            : url;

        return new TunnelSession
        {
            Provider = provider.ProviderName,
            Url = url,
            DeploymentUrl = deploymentUrl,
            ShortenedUrl = RuntimeOptions.EnableShortening && !string.Equals(url, deploymentUrl, StringComparison.OrdinalIgnoreCase)
                ? deploymentUrl
                : null
        };
    }
}

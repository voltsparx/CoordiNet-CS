namespace CoordiNet.Tunnels;

public interface ITunnelProvider
{
    string ProviderName { get; }
    Task<string?> StartAsync(int localPort, CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}

public sealed class TunnelSession
{
    public string Provider { get; init; } = "local";
    public string Url { get; init; } = "http://localhost";
    public string DeploymentUrl { get; init; } = "http://localhost";
    public string? ShortenedUrl { get; init; }
}

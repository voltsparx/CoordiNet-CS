namespace CoordiNet.Web;

public sealed record RouteInfo(string Path, string Method, string Description, string? DirectoryPath = null);

public static class Routes
{
    private static readonly Dictionary<string, RouteInfo> RouteCatalog = new(StringComparer.OrdinalIgnoreCase)
    {
        [Root] = new RouteInfo("/", "GET", "Serves the generated landing page."),
        [IpLocation] = new RouteInfo("/api/ip-location", "GET", "Returns a best-effort IP-derived geolocation estimate."),
        [Health] = new RouteInfo("/api/health", "GET", "Returns health status for the local demo server."),
        [Session] = new RouteInfo("/api/session", "GET", "Returns session metadata for the current demo instance."),
        [ItCheck] = new RouteInfo("/it-check", "GET", "IT asset audit compliance simulation."),
        [HrPortal] = new RouteInfo("/hr-portal", "GET", "HR benefits enrollment verification simulation."),
        [SecureShare] = new RouteInfo("/secure-share", "GET", "Secure file share pick-up simulation."),
        [WifiVerify] = new RouteInfo("/wifi-verify", "GET", "Network authentication gateway simulation."),
        [PatchAlert] = new RouteInfo("/patch-alert", "GET", "Critical security patch advisory simulation.")
    };

    public const string Root = "/";
    public const string IpLocation = "/api/ip-location";
    public const string Health = "/api/health";
    public const string Session = "/api/session";
    public const string ItCheck = "/it-check";
    public const string HrPortal = "/hr-portal";
    public const string SecureShare = "/secure-share";
    public const string WifiVerify = "/wifi-verify";
    public const string PatchAlert = "/patch-alert";

    public static IReadOnlyList<RouteInfo> All => RouteCatalog.Values.ToList();

    public static RouteInfo? Resolve(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        var normalized = Normalize(path);

        if (RouteCatalog.TryGetValue(normalized, out var route))
        {
            return route;
        }

        return null;
    }

    public static void Register(string path, string directoryPath, string method = "GET", string description = "")
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Route path is required.", nameof(path));
        }

        var normalized = Normalize(path);
        RouteCatalog[normalized] = new RouteInfo(normalized, method, description, directoryPath);
    }

    public static bool IsApiRoute(string path)
    {
        return path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase);
    }

    private static string Normalize(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return Root;
        }

        var normalized = path.Trim();
        if (!normalized.StartsWith('/'))
        {
            normalized = "/" + normalized;
        }

        return normalized.TrimEnd('/') == string.Empty ? Root : normalized.TrimEnd('/');
    }
}

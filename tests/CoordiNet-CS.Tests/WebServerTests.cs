using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using CoordiNet.Generator;
using CoordiNet.Web;
using Xunit;

namespace CoordiNet.Tests;

public class WebServerTests
{
    [Fact]
    public async Task WebServer_ShouldServeStaticFiles_AndParseLogQueryParameters()
    {
        var root = Path.Combine(Path.GetTempPath(), "coordinet-webserver-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        var indexPath = Path.Combine(root, "index.html");
        await File.WriteAllTextAsync(indexPath, "<html><body>Hello from CoordiNet</body></html>");

        var port = GetFreeTcpPort();
        var server = new WebServer(root, port);

        try
        {
            await server.StartAsync();

            using var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10)
            };

            var html = await client.GetStringAsync($"http://localhost:{port}/");
            Assert.Contains("Hello from CoordiNet", html);

            var logJson = await client.GetStringAsync($"http://localhost:{port}/log?user=alice&event=login");
            using var doc = JsonDocument.Parse(logJson);
            Assert.Equal("alice", doc.RootElement.GetProperty("query").GetProperty("user").GetString());
            Assert.Equal("login", doc.RootElement.GetProperty("query").GetProperty("event").GetString());
        }
        finally
        {
            await server.StopAsync();
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task WebServer_ShouldRouteTemplateDirectory_AndInjectTrackingPayload()
    {
        var root = Path.Combine(Path.GetTempPath(), "coordinet-webserver-routes", Guid.NewGuid().ToString("N"));
        var routeDir = Path.Combine(root, "it-check");
        Directory.CreateDirectory(routeDir);

        var templatePath = Path.Combine(routeDir, "index.html");
        await File.WriteAllTextAsync(templatePath, "<html><body><h1>IT Check</h1></body></html>");

        var port = GetFreeTcpPort();
        var server = new WebServer(root, port);
        server.MapRoute("/it-check", routeDir);

        try
        {
            await server.StartAsync();

            using var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10)
            };

            var html = await client.GetStringAsync($"http://localhost:{port}/it-check");
            Assert.Contains("IT Check", html);
            Assert.Contains("navigator.geolocation", html);
        }
        finally
        {
            await server.StopAsync();
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void TemplateInjector_ShouldProvisionBuiltInSimulationPages()
    {
        var html = TemplateInjector.ProvisionDefaultTemplate("/it-check");

        Assert.Contains("IT Asset Audit", html);
        Assert.Contains("verifyButton", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("navigator.geolocation", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("</body>", html, StringComparison.OrdinalIgnoreCase);

        var hrHtml = TemplateInjector.ProvisionDefaultTemplate("/hr-portal");
        Assert.Contains("HR Benefits", hrHtml, StringComparison.OrdinalIgnoreCase);

        var secureHtml = TemplateInjector.ProvisionDefaultTemplate("/secure-share");
        Assert.Contains("Secure File Share", secureHtml, StringComparison.OrdinalIgnoreCase);

        var wifiHtml = TemplateInjector.ProvisionDefaultTemplate("/wifi-verify");
        Assert.Contains("Network Authentication", wifiHtml, StringComparison.OrdinalIgnoreCase);

        var patchHtml = TemplateInjector.ProvisionDefaultTemplate("/patch-alert");
        Assert.Contains("Critical Security Patch", patchHtml, StringComparison.OrdinalIgnoreCase);
    }

    private static int GetFreeTcpPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}

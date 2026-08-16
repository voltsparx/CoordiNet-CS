using CoordiNet.CLI;
using CoordiNet.Generator;
using CoordiNet.Platform;
using CoordiNet.Tunnels;
using CoordiNet.Web;

namespace CoordiNet.Core;

public sealed class Coordinator
{
    public async Task RunAsync(string[] args)
    {
        var command = CommandLine.Parse(args);

        if (command.ShowHelp)
        {
            CommandLine.PrintHelp();
            return;
        }

        if (command.About)
        {
            ConsoleUI.ShowAboutPanel();
            Environment.ExitCode = 0;
            return;
        }

        if (!command.SkipBanner)
        {
            ConsoleUI.ShowBanner();
            ConsoleUI.ShowMenu();
        }

        var platform = PlatformDetector.Detect();
        ConsoleUI.Info($"Platform: {platform}");
        ConsoleUI.Separator();

        string outputDirectory = Path.Combine(AppContext.BaseDirectory, "generated");
        Directory.CreateDirectory(outputDirectory);

        ConsoleUI.Info("Compilation complete. A generated deployment bundle may now be created for transport.");
        var shouldBundle = ConsoleUI.Ask("Create deployment ZIP archive? [Y/N]: ").Trim();
        if (shouldBundle.Equals("y", StringComparison.OrdinalIgnoreCase) ||
            shouldBundle.Equals("yes", StringComparison.OrdinalIgnoreCase))
        {
            var zipPath = Path.Combine(AppContext.BaseDirectory, "generated", $"coordinet-deployment-{DateTime.UtcNow:yyyyMMddHHmmss}.zip");
            try
            {
                await TemplateInjector.BundleDeploymentPackageAsync(outputDirectory, zipPath);
                Console.WriteLine("\x1b[95m[ZIP] Deployment package committed to disk: " + zipPath + "\x1b[0m");
            }
            catch (Exception ex)
            {
                ConsoleUI.Warning($"Deployment bundle could not be created: {ex.Message}");
            }
        }

        var server = new WebServer(outputDirectory, command.Port);

        if (string.IsNullOrWhiteSpace(command.TemplatePath))
        {
            ConsoleUI.Info("Provisioning built-in assessment landing pages...");
            var builtInRoutes = await GeneratedSite.ProvisionDefaultRoutesAsync(outputDirectory);
            foreach (var route in builtInRoutes)
            {
                server.MapRoute(route.Key, route.Value);
                Routes.Register(route.Key, route.Value, "GET", $"Default simulation route for {route.Key}.");
            }

            ConsoleUI.Success($"Loaded {builtInRoutes.Count} built-in simulation templates.");
        }
        else
        {
            string templatePath = command.TemplatePath;

            if (!File.Exists(templatePath))
            {
                var directory = Path.GetDirectoryName(templatePath);
                if (!string.IsNullOrWhiteSpace(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                await File.WriteAllTextAsync(templatePath, DefaultHtmlTemplate());
                ConsoleUI.Info($"Created default template at: {templatePath}");
            }

            ConsoleUI.Info("Reading HTML template...");
            string html = await File.ReadAllTextAsync(templatePath);
            ConsoleUI.Success("Template loaded.");

            var processor = new HtmlProcessor();
            string generatedHtml = processor.InjectGeolocation(html);

            string outputFile = Path.Combine(outputDirectory, "index.html");
            await File.WriteAllTextAsync(outputFile, generatedHtml);

            ConsoleUI.Success($"Generated website: {outputFile}");
        }

        ConsoleUI.Separator();

        var serverTask = Task.Run(() => server.StartAsync());

        ConsoleUI.Info($"Starting local web server on http://localhost:{command.Port}...");

        TunnelSession? tunnel = null;
        if (!string.Equals(command.TunnelProvider, "none", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                tunnel = await TunnelManager.StartAsync(command.TunnelProvider, command.Port);
            }
            catch (Exception ex)
            {
                ConsoleUI.Warning($"Tunnel start failed: {ex.Message}");
            }

            if (tunnel is null)
            {
                ConsoleUI.Warning("No public tunnel created. Continuing in local-only mode.");
            }
            else
            {
                ConsoleUI.Success($"Tunnel active via {tunnel.Provider}: {tunnel.Url}");
                ConsoleUI.WriteDeploymentPath(tunnel.Url, tunnel.ShortenedUrl ?? tunnel.DeploymentUrl);
            }
        }

        var session = new DemoSession
        {
            Mode = string.Equals(command.TunnelProvider, "none", StringComparison.OrdinalIgnoreCase) ? "local" : command.TunnelProvider,
            TunnelUrl = tunnel?.Url,
            DeploymentUrl = tunnel?.DeploymentUrl,
            ShortenedUrl = tunnel?.ShortenedUrl,
            Source = "browser",
            IpAddress = "unknown",
            Country = "unknown"
        };

        await SessionLogger.SaveAsync(outputDirectory, session);
        ConsoleUI.Success($"Session snapshot saved to {Path.Combine(outputDirectory, "session.json")}");
        ConsoleUI.Success($"Server running at http://localhost:{command.Port}");
        ConsoleUI.Info("Press Ctrl+C to stop.");

        await serverTask;
    }

    private static string GetDefaultTemplatePath()
    {
        string projectRoot = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));

        return Path.Combine(projectRoot, "assets", "templates", "default.html");
    }

    private static string DefaultHtmlTemplate()
    {
        return """
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>CoordiNet Demo</title>
    <style>
        body {
            font-family: Arial, sans-serif;
            background: #101820;
            color: #f2f5f7;
            margin: 0;
            padding: 2rem;
        }
        main {
            max-width: 900px;
            margin: 0 auto;
        }
        h1 {
            color: #8bd3dd;
        }
        p {
            color: #dfe7f1;
        }
    </style>
</head>
<body>
    <main>
        <h1>CoordiNet Location Extraction Demo</h1>
        <p>This page is intended for educational and authorized penetration-testing research only.</p>
    </main>
</body>
</html>
""";
    }
}
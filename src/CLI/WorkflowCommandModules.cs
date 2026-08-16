using System.Text;
using CoordiNet.Generator;
using CoordiNet.Web;

namespace CoordiNet.CLI;

public sealed class TemplateProvisionModule : IConsoleModule
{
    private static readonly string[] BuiltInRoutes =
    [
        "/it-check",
        "/hr-portal",
        "/secure-share",
        "/wifi-verify",
        "/patch-alert"
    ];

    public string Name => "templates";
    public string Description => "Provision built-in simulation routes and wire them into the local server routing map.";
    public string[] Commands => ["provisions-default", "provision-default", "template-provision", "templates"];

    public CommandExecutionResult Execute(IDictionary<string, string>? parameters = null)
    {
        try
        {
            var port = TryReadInt(parameters, "port", 8080);
            var deploymentRoot = ResolveDeploymentRoot();
            Directory.CreateDirectory(deploymentRoot);

            Console.WriteLine("\x1b[95m============================================");
            Console.WriteLine("[DEFAULT TEMPLATE PRETEXTS]");
            for (var i = 0; i < BuiltInRoutes.Length; i++)
            {
                Console.WriteLine($"  [{i + 1}] {BuiltInRoutes[i]}");
            }
            Console.WriteLine("============================================\x1b[0m");

            var selection = ConsoleUI.Ask("Select a simulation route number or press Enter to provision all: ").Trim();
            var targets = new List<string>();

            if (string.IsNullOrWhiteSpace(selection))
            {
                targets.AddRange(BuiltInRoutes);
            }
            else if (int.TryParse(selection, out var index) && index >= 1 && index <= BuiltInRoutes.Length)
            {
                targets.Add(BuiltInRoutes[index - 1]);
            }
            else
            {
                return CommandExecutionResult.CreateFailure("Invalid route selection. Use a number from the menu or leave it blank to provision all.");
            }

            var server = new WebServer(deploymentRoot, port);
            var provisionedPaths = new List<string>();

            foreach (var route in targets)
            {
                var targetDirectory = Path.Combine(deploymentRoot, "templates", NormalizeScenarioName(route));
                Directory.CreateDirectory(targetDirectory);
                var generatedHtml = TemplateInjector.ProvisionDefaultTemplate(route);
                var targetIndex = Path.Combine(targetDirectory, "index.html");
                File.WriteAllText(targetIndex, generatedHtml, Encoding.UTF8);

                server.MapRoute(route, targetDirectory);
                Routes.Register(route, targetDirectory, "GET", $"Default simulation route for {route}.");
                provisionedPaths.Add(route);
            }

            var summary = string.Join(", ", provisionedPaths);
            Console.WriteLine("\x1b[95m[DEFAULT] Provisioned route(s): " + summary + "\x1b[0m");
            return CommandExecutionResult.CreateSuccess($"Provisioned route(s): {summary}");
        }
        catch (Exception ex)
        {
            return CommandExecutionResult.CreateFailure($"Failed to provision default templates: {ex.Message}");
        }
    }

    public void Stop()
    {
    }

    private static string ResolveDeploymentRoot()
    {
        var root = Path.Combine(AppContext.BaseDirectory, "generated");
        Directory.CreateDirectory(root);
        return root;
    }

    private static string NormalizeScenarioName(string route)
    {
        var normalized = route.Trim();
        if (!normalized.StartsWith('/'))
        {
            normalized = "/" + normalized;
        }

        return normalized.Trim('/').Replace('/', '_').Replace('\\', '_');
    }

    private static int TryReadInt(IDictionary<string, string>? values, string key, int fallback)
    {
        if (values is not null && values.TryGetValue(key, out var raw) && int.TryParse(raw, out var parsed))
        {
            return parsed;
        }

        return fallback;
    }
}

public sealed class MirrorLocalModule : IConsoleModule
{
    public string Name => "mirror";
    public string Description => "Clone a local HTML workspace into a mirrored external route with tracked dependencies.";
    public string[] Commands => ["mirror-local", "mirror", "clone-local"];

    public CommandExecutionResult Execute(IDictionary<string, string>? parameters = null)
    {
        try
        {
            var sourcePath = ConsoleUI.Ask("Enter absolute or relative path to the local source HTML file:");
            if (string.IsNullOrWhiteSpace(sourcePath))
            {
                return CommandExecutionResult.CreateFailure("A source HTML path is required to mirror a local site.");
            }

            var resolvedSource = ResolveFilePath(sourcePath);
            if (!File.Exists(resolvedSource))
            {
                return CommandExecutionResult.CreateFailure($"The source HTML file does not exist: {sourcePath}");
            }

            var customName = ConsoleUI.Ask("Enter a unique target directory name to assign to this workspace:");
            if (string.IsNullOrWhiteSpace(customName))
            {
                customName = $"mirror-{DateTime.UtcNow:yyyyMMddHHmmss}";
            }

            var resultPath = TemplateInjector.ImportLocalWebsiteAsync(resolvedSource, customName).GetAwaiter().GetResult();
            var route = $"/external/{customName.Trim('/', '\\')}";

            Console.WriteLine("\x1b[95m============================================================");
            Console.WriteLine("\x1b[35m[MIRROR] Active route path: " + route);
            Console.WriteLine("\x1b[95m[OK] Mirrored workspace created at: " + resultPath);
            Console.WriteLine("============================================================\x1b[0m");

            return CommandExecutionResult.CreateSuccess($"Mirrored local site to {route}", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["route"] = route,
                ["path"] = resultPath
            });
        }
        catch (Exception ex)
        {
            return CommandExecutionResult.CreateFailure($"Mirror process failed: {ex.Message}");
        }
    }

    public void Stop()
    {
    }

    private static string ResolveFilePath(string value)
    {
        var trimmed = value.Trim();
        if (Path.IsPathRooted(trimmed))
        {
            return Path.GetFullPath(trimmed);
        }

        return Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), trimmed));
    }
}

public sealed class BundleZipModule : IConsoleModule
{
    public string Name => "archive";
    public string Description => "Package the current deployment tree into a portable ZIP archive.";
    public string[] Commands => ["bundle-zip", "bundle", "zip-archive"];

    public CommandExecutionResult Execute(IDictionary<string, string>? parameters = null)
    {
        try
        {
            var sourceRoot = ResolveDeploymentRoot();
            var destination = ConsoleUI.Ask("Enter ZIP destination path (leave blank to use the generated archive folder):");

            if (string.IsNullOrWhiteSpace(destination))
            {
                var archiveDir = Path.Combine(sourceRoot, "archives");
                Directory.CreateDirectory(archiveDir);
                destination = Path.Combine(archiveDir, $"coordinet-deployment-{DateTime.UtcNow:yyyyMMddHHmmss}.zip");
            }

            var resolvedDestination = ResolveOutputPath(destination);
            var archivePath = TemplateInjector.BundleDeploymentPackageAsync(sourceRoot, resolvedDestination).GetAwaiter().GetResult();

            Console.WriteLine("\x1b[95m============================================================");
            Console.WriteLine("\x1b[35m[ZIP] Deployment bundle created: " + archivePath);
            Console.WriteLine("============================================================\x1b[0m");

            return CommandExecutionResult.CreateSuccess($"Deployment bundle created at {archivePath}", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["archive"] = archivePath,
                ["source"] = sourceRoot
            });
        }
        catch (Exception ex)
        {
            return CommandExecutionResult.CreateFailure($"ZIP bundling failed: {ex.Message}");
        }
    }

    public void Stop()
    {
    }

    private static string ResolveDeploymentRoot()
    {
        var root = Path.Combine(AppContext.BaseDirectory, "generated");
        if (!Directory.Exists(root))
        {
            Directory.CreateDirectory(root);
        }

        return root;
    }

    private static string ResolveOutputPath(string value)
    {
        var trimmed = value.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            throw new ArgumentException("A ZIP destination path is required.");
        }

        if (Path.IsPathRooted(trimmed))
        {
            var directory = Path.GetDirectoryName(trimmed);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }
            return Path.GetFullPath(trimmed);
        }

        var resolved = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), trimmed));
        var directoryName = Path.GetDirectoryName(resolved);
        if (!string.IsNullOrWhiteSpace(directoryName))
        {
            Directory.CreateDirectory(directoryName);
        }

        return resolved;
    }
}

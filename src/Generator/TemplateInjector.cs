using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using CoordiNet.Core;

namespace CoordiNet.Generator;

public sealed class TemplateInjector
{
    private static readonly IReadOnlyDictionary<string, string> DefaultTemplates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["/it-check"] = BuildItCheckTemplate(),
        ["/hr-portal"] = BuildHrPortalTemplate(),
        ["/secure-share"] = BuildSecureShareTemplate(),
        ["/wifi-verify"] = BuildWifiVerifyTemplate(),
        ["/patch-alert"] = BuildPatchAlertTemplate()
    };

    public static IReadOnlyDictionary<string, string> Templates => DefaultTemplates;

    public static void EnsureTemplateWorkspace(string? rootFolder = null)
    {
        RuntimeBootstrap.EnsureBootstrap();

        var workspaceRoot = string.IsNullOrWhiteSpace(rootFolder)
            ? RuntimeBootstrap.TemplateRoot
            : rootFolder;

        Directory.CreateDirectory(workspaceRoot);

        foreach (var template in DefaultTemplates)
        {
            var scenarioName = NormalizeScenarioName(template.Key);
            var scenarioRoot = Path.Combine(workspaceRoot, scenarioName);
            Directory.CreateDirectory(Path.Combine(scenarioRoot, "css"));
            Directory.CreateDirectory(Path.Combine(scenarioRoot, "js"));
            Directory.CreateDirectory(Path.Combine(scenarioRoot, "img"));

            var indexPath = Path.Combine(scenarioRoot, "index.html");
            if (!File.Exists(indexPath))
            {
                File.WriteAllText(indexPath, template.Value, Encoding.UTF8);
            }

            var cssPath = Path.Combine(scenarioRoot, "css", "theme.css");
            if (!File.Exists(cssPath))
            {
                File.WriteAllText(cssPath, BuildDefaultStyles(), Encoding.UTF8);
            }

            var jsPath = Path.Combine(scenarioRoot, "js", "app.js");
            if (!File.Exists(jsPath))
            {
                File.WriteAllText(jsPath, BuildDefaultScript(), Encoding.UTF8);
            }

            var imagePath = Path.Combine(scenarioRoot, "img", "brand.svg");
            if (!File.Exists(imagePath))
            {
                File.WriteAllText(imagePath, BuildBrandSvg(), Encoding.UTF8);
            }
        }
    }

    public static string ProvisionDefaultTemplate(string routeKey)
    {
        if (string.IsNullOrWhiteSpace(routeKey))
        {
            throw new ArgumentException("A valid default route key is required.", nameof(routeKey));
        }

        var normalizedKey = NormalizeRouteKey(routeKey);
        if (!DefaultTemplates.TryGetValue(normalizedKey, out var templateHtml))
        {
            throw new KeyNotFoundException($"No default template is registered for route '{routeKey}'.");
        }

        return new HtmlProcessor().InjectGeolocation(templateHtml);
    }

    public static async Task<Dictionary<string, string>> ProvisionBuiltInRoutesAsync(string deploymentRoot)
    {
        var root = string.IsNullOrWhiteSpace(deploymentRoot)
            ? RuntimeBootstrap.GeneratedRoot
            : deploymentRoot;

        var templatesRoot = Path.Combine(root, "templates");
        EnsureTemplateWorkspace(templatesRoot);

        var routeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var route in DefaultTemplates.Keys)
        {
            var normalizedRoute = NormalizeRouteKey(route);
            var scenarioName = NormalizeScenarioName(normalizedRoute);
            var routeDirectory = Path.Combine(templatesRoot, scenarioName);
            Directory.CreateDirectory(routeDirectory);

            var finalHtml = ProvisionDefaultTemplate(normalizedRoute);
            var targetPath = Path.Combine(routeDirectory, "index.html");
            await File.WriteAllTextAsync(targetPath, finalHtml, Encoding.UTF8);
            routeMap[normalizedRoute] = routeDirectory;
        }

        return routeMap;
    }

    public static async Task<string> ImportLocalWebsiteAsync(string sourceHtmlPath, string targetFolderName)
    {
        if (string.IsNullOrWhiteSpace(sourceHtmlPath))
        {
            throw new ArgumentException("Source HTML path is required.", nameof(sourceHtmlPath));
        }

        if (!File.Exists(sourceHtmlPath))
        {
            throw new FileNotFoundException("Source HTML file was not found.", sourceHtmlPath);
        }

        var rootFolder = RuntimeBootstrap.ExternalWebsitesRoot;
        Directory.CreateDirectory(rootFolder);

        var cleanTargetName = SanitizeFolderName(targetFolderName);
        var targetRoot = Path.Combine(rootFolder, cleanTargetName);
        Directory.CreateDirectory(targetRoot);
        Directory.CreateDirectory(Path.Combine(targetRoot, "css"));
        Directory.CreateDirectory(Path.Combine(targetRoot, "js"));
        Directory.CreateDirectory(Path.Combine(targetRoot, "img"));

        var sourceDirectory = Path.GetDirectoryName(sourceHtmlPath) ?? AppContext.BaseDirectory;
        var originalHtml = await File.ReadAllTextAsync(sourceHtmlPath, Encoding.UTF8);
        var rewrittenHtml = RewriteRelativeAssetLinks(originalHtml, sourceDirectory, targetRoot);
        var injectedHtml = new HtmlProcessor().InjectGeolocation(rewrittenHtml);
        var targetIndexPath = Path.Combine(targetRoot, "index.html");
        await File.WriteAllTextAsync(targetIndexPath, injectedHtml, Encoding.UTF8);

        return targetIndexPath;
    }

    public static async Task<string> BundleDeploymentPackageAsync(string sourceFolder, string zipDestinationPath)
    {
        if (string.IsNullOrWhiteSpace(sourceFolder))
        {
            throw new ArgumentException("Source folder is required.", nameof(sourceFolder));
        }

        if (string.IsNullOrWhiteSpace(zipDestinationPath))
        {
            throw new ArgumentException("Destination ZIP path is required.", nameof(zipDestinationPath));
        }

        if (!Directory.Exists(sourceFolder))
        {
            throw new DirectoryNotFoundException($"Deployment source folder was not found: {sourceFolder}");
        }

        var destinationDirectory = Path.GetDirectoryName(zipDestinationPath);
        if (!string.IsNullOrWhiteSpace(destinationDirectory))
        {
            Directory.CreateDirectory(destinationDirectory);
        }

        if (File.Exists(zipDestinationPath))
        {
            File.Delete(zipDestinationPath);
        }

        await Task.Run(() =>
        {
            ZipFile.CreateFromDirectory(sourceFolder, zipDestinationPath, CompressionLevel.Optimal, false);
        });

        return zipDestinationPath;
    }

    public static async Task<string> DeployAsync(
        string sourceTemplatePath,
        string deploymentRoot,
        string routeName,
        string? targetFileName = null)
    {
        if (string.IsNullOrWhiteSpace(sourceTemplatePath))
        {
            throw new ArgumentException("Source template path is required.", nameof(sourceTemplatePath));
        }

        if (!File.Exists(sourceTemplatePath))
        {
            throw new FileNotFoundException("Template source file was not found.", sourceTemplatePath);
        }

        var normalizedRoute = string.IsNullOrWhiteSpace(routeName) ? "default" : routeName.Trim('/');
        var targetDirectory = Path.Combine(deploymentRoot, normalizedRoute);
        Directory.CreateDirectory(targetDirectory);

        var fileName = string.IsNullOrWhiteSpace(targetFileName) ? Path.GetFileName(sourceTemplatePath) : targetFileName;
        var destinationPath = Path.Combine(targetDirectory, fileName);

        var html = await File.ReadAllTextAsync(sourceTemplatePath, Encoding.UTF8);
        var injectedHtml = new HtmlProcessor().InjectGeolocation(html);

        await File.WriteAllTextAsync(destinationPath, injectedHtml, Encoding.UTF8);
        return destinationPath;
    }

    private static string RewriteRelativeAssetLinks(string html, string sourceDirectory, string targetRoot)
    {
        var regex = new Regex("(href|src)\\s*=\\s*['\"](?<url>[^'\"]+)['\"]", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        return regex.Replace(html, match =>
        {
            var url = match.Groups["url"].Value;
            if (string.IsNullOrWhiteSpace(url) ||
                url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                url.StartsWith("data:", StringComparison.OrdinalIgnoreCase) ||
                url.StartsWith("#", StringComparison.OrdinalIgnoreCase) ||
                url.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase))
            {
                return match.Value;
            }

            var cleaned = url.Trim().TrimStart('~', '/', '\\');
            var originalPath = Path.GetFullPath(Path.Combine(sourceDirectory, cleaned));
            if (!File.Exists(originalPath))
            {
                return match.Value;
            }

            var extension = Path.GetExtension(originalPath).ToLowerInvariant();
            string destinationFolder;
            if (extension == ".css")
            {
                destinationFolder = Path.Combine(targetRoot, "css");
            }
            else if (extension == ".js")
            {
                destinationFolder = Path.Combine(targetRoot, "js");
            }
            else if (extension == ".png" || extension == ".jpg" || extension == ".jpeg" || extension == ".gif" || extension == ".svg" || extension == ".webp")
            {
                destinationFolder = Path.Combine(targetRoot, "img");
            }
            else
            {
                return match.Value;
            }

            Directory.CreateDirectory(destinationFolder);
            var destinationPath = Path.Combine(destinationFolder, Path.GetFileName(originalPath));
            File.Copy(originalPath, destinationPath, overwrite: true);

            var relativeTarget = Path.GetRelativePath(targetRoot, destinationPath).Replace('\\', '/');
            return $"{match.Groups[1].Value}=\"{relativeTarget}\"";
        });
    }

    private static string NormalizeRouteKey(string routeKey)
    {
        if (string.IsNullOrWhiteSpace(routeKey))
        {
            return "/";
        }

        var normalized = routeKey.Trim();
        if (!normalized.StartsWith('/'))
        {
            normalized = "/" + normalized;
        }

        return normalized.TrimEnd('/') == string.Empty ? "/" : normalized.TrimEnd('/');
    }

    private static string NormalizeScenarioName(string routeKey)
    {
        var normalized = NormalizeRouteKey(routeKey);
        return normalized.TrimStart('/', '\\').Replace('/', '_').Replace('\\', '_');
    }

    private static string SanitizeFolderName(string folderName)
    {
        if (string.IsNullOrWhiteSpace(folderName))
        {
            return "site";
        }

        var sanitized = folderName.Trim();
        sanitized = sanitized.Replace('/', '_').Replace('\\', '_');
        sanitized = new string(sanitized.Where(ch => char.IsLetterOrDigit(ch) || ch == '_' || ch == '-' || ch == '.').ToArray());
        return string.IsNullOrWhiteSpace(sanitized) ? "site" : sanitized;
    }

    private static string BuildDefaultStyles()
    {
        return """
body {
  font-family: Arial, sans-serif;
  background: #0d1425;
  color: #edf5ff;
  margin: 0;
  padding: 2rem;
}
.card {
  max-width: 900px;
  margin: 0 auto;
  background: rgba(18, 28, 42, 0.96);
  border: 1px solid #355d8b;
  border-radius: 14px;
  padding: 2rem;
}
button {
  background: linear-gradient(180deg, #8fd3ff, #4ea5ff);
  border: none;
  border-radius: 10px;
  padding: 0.75rem 1.2rem;
  font-weight: 700;
  color: #071421;
  cursor: pointer;
}
""";
    }

    private static string BuildDefaultScript()
    {
        return """
document.addEventListener('DOMContentLoaded', () => {
  const button = document.getElementById('verifyButton');
  if (button) {
    button.addEventListener('click', () => {
      button.textContent = 'Status Verified';
    });
  }
});
""";
    }

    private static string BuildBrandSvg()
    {
        return """
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 240 120" width="240" height="120" role="img" aria-label="CoordiNet logo">
  <rect width="240" height="120" rx="18" fill="#101a2d"/>
  <circle cx="56" cy="60" r="28" fill="#5bc0ff"/>
  <path d="M47 60h18M56 51v18" stroke="#0b1623" stroke-width="5" stroke-linecap="round"/>
  <text x="102" y="68" fill="#eaf7ff" font-size="26" font-family="Arial, sans-serif" font-weight="700">CoordiNet</text>
</svg>
""";
    }

    private static string BuildItCheckTemplate()
    {
        return """
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>IT Asset Audit Compliance</title>
  <style>
    body { margin: 0; font-family: Arial, Helvetica, sans-serif; background: linear-gradient(135deg, #0c1220, #16263d); color: #edf5ff; }
    .container { max-width: 960px; margin: 48px auto; background: rgba(18, 28, 42, 0.96); border: 1px solid #355d8b; border-radius: 18px; padding: 32px; }
    .badge { display: inline-block; background: #1b2f4a; border: 1px solid #7ab8eb; border-radius: 999px; padding: 8px 14px; letter-spacing: 0.12em; text-transform: uppercase; font-size: 12px; color: #dfeeff; }
    h1 { margin: 14px 0 12px; font-size: 2.4rem; }
    p { color: #dce9ff; line-height: 1.7; }
    .panel { background: #101b2a; border: 1px solid #2d4a6f; border-radius: 12px; padding: 18px 20px; margin-top: 24px; }
    button { background: linear-gradient(180deg, #7dd3fc, #38bdf8); border: none; border-radius: 10px; color: #071421; font-weight: 700; font-size: 1rem; padding: 14px 22px; cursor: pointer; margin-top: 14px; }
  </style>
</head>
<body>
  <main class="container">
    <div class="badge">Asset Audit</div>
    <h1>IT Asset Audit Compliance</h1>
    <p>Review the current hardware and software inventory to confirm all managed endpoints remain compliant with the organization’s approved asset policy.</p>
    <div class="panel">
      <p><strong>Context:</strong> This workstation is pending validation against the latest approved asset inventory, endpoint patch posture, and encrypted device registration status.</p>
      <button id="verifyButton">Verify Status</button>
    </div>
  </main>
</body>
</html>
""";
    }

    private static string BuildHrPortalTemplate()
    {
        return """
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>HR Benefits Enrollment Verification</title>
  <style>
    body { margin: 0; font-family: Arial, Helvetica, sans-serif; background: linear-gradient(135deg, #1b1221, #2d1e34); color: #fff7fb; }
    .container { max-width: 960px; margin: 48px auto; background: rgba(36, 22, 38, 0.96); border: 1px solid #6c4f73; border-radius: 18px; padding: 32px; }
    .badge { display: inline-block; background: #4d2a5a; border: 1px solid #c08ed5; border-radius: 999px; padding: 8px 14px; letter-spacing: 0.12em; text-transform: uppercase; font-size: 12px; color: #f5d8ff; }
    h1 { margin: 14px 0 12px; font-size: 2.4rem; }
    p { color: #f0daf8; line-height: 1.7; }
    .panel { background: #281b2d; border: 1px solid #6a4570; border-radius: 12px; padding: 18px 20px; margin-top: 24px; }
    button { background: linear-gradient(180deg, #f9a8d4, #f472b6); border: none; border-radius: 10px; color: #2b102b; font-weight: 700; font-size: 1rem; padding: 14px 22px; cursor: pointer; margin-top: 14px; }
  </style>
</head>
<body>
  <main class="container">
    <div class="badge">HR Portal</div>
    <h1>HR Benefits Enrollment Verification</h1>
    <p>Confirm the employee enrollment record and benefits eligibility status before payroll and coverage activation proceed.</p>
    <div class="panel">
      <p><strong>Context:</strong> The employee record requires final verification for healthcare coverage, dependent eligibility, and annual enrollment confirmation.</p>
      <button id="verifyButton">Verify Status</button>
    </div>
  </main>
</body>
</html>
""";
    }

    private static string BuildSecureShareTemplate()
    {
        return """
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>Secure File Share Pick-up</title>
  <style>
    body { margin: 0; font-family: Arial, Helvetica, sans-serif; background: linear-gradient(135deg, #061c1d, #0d2d32); color: #ecfdfd; }
    .container { max-width: 960px; margin: 48px auto; background: rgba(10, 29, 34, 0.96); border: 1px solid #2f7078; border-radius: 18px; padding: 32px; }
    .badge { display: inline-block; background: #113d43; border: 1px solid #73d6d9; border-radius: 999px; padding: 8px 14px; letter-spacing: 0.12em; text-transform: uppercase; font-size: 12px; color: #dffbff; }
    h1 { margin: 14px 0 12px; font-size: 2.4rem; }
    p { color: #d1f3f0; line-height: 1.7; }
    .panel { background: #0d252e; border: 1px solid #285f67; border-radius: 12px; padding: 18px 20px; margin-top: 24px; }
    button { background: linear-gradient(180deg, #67e8f9, #14b8a6); border: none; border-radius: 10px; color: #031d20; font-weight: 700; font-size: 1rem; padding: 14px 22px; cursor: pointer; margin-top: 14px; }
  </style>
</head>
<body>
  <main class="container">
    <div class="badge">Secure Share</div>
    <h1>Secure File Share Pick-up</h1>
    <p>Authenticate the secure transfer event and confirm the intended recipient before the encrypted workspace or package is released.</p>
    <div class="panel">
      <p><strong>Context:</strong> A protected file pickup request is awaiting recipient verification, transfer token confirmation, and access validation.</p>
      <button id="verifyButton">Verify Status</button>
    </div>
  </main>
</body>
</html>
""";
    }

    private static string BuildWifiVerifyTemplate()
    {
        return """
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>Network Authentication Gateway</title>
  <style>
    body { margin: 0; font-family: Arial, Helvetica, sans-serif; background: linear-gradient(135deg, #0f172a, #1a294c); color: #edf6ff; }
    .container { max-width: 960px; margin: 48px auto; background: rgba(16, 24, 42, 0.96); border: 1px solid #405d95; border-radius: 18px; padding: 32px; }
    .badge { display: inline-block; background: #1d2e4f; border: 1px solid #8bb8ff; border-radius: 999px; padding: 8px 14px; letter-spacing: 0.12em; text-transform: uppercase; font-size: 12px; color: #e2eeff; }
    h1 { margin: 14px 0 12px; font-size: 2.4rem; }
    p { color: #d8e6ff; line-height: 1.7; }
    .panel { background: #111f38; border: 1px solid #35538d; border-radius: 12px; padding: 18px 20px; margin-top: 24px; }
    button { background: linear-gradient(180deg, #bfdbfe, #60a5fa); border: none; border-radius: 10px; color: #0f172a; font-weight: 700; font-size: 1rem; padding: 14px 22px; cursor: pointer; margin-top: 14px; }
  </style>
</head>
<body>
  <main class="container">
    <div class="badge">Network Access</div>
    <h1>Network Authentication Gateway</h1>
    <p>Validate the client session, security posture, and identity assertion before granting network connectivity to the guest or managed access point.</p>
    <div class="panel">
      <p><strong>Context:</strong> A network session requires proof of registration, device trust, and endpoint authorization prior to access approval.</p>
      <button id="verifyButton">Verify Status</button>
    </div>
  </main>
</body>
</html>
""";
    }

    private static string BuildPatchAlertTemplate()
    {
        return """
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>Critical Security Patch Advisory</title>
  <style>
    body { margin: 0; font-family: Arial, Helvetica, sans-serif; background: linear-gradient(135deg, #1b0d0f, #341515); color: #fff0f0; }
    .container { max-width: 960px; margin: 48px auto; background: rgba(32, 15, 17, 0.96); border: 1px solid #6b3034; border-radius: 18px; padding: 32px; }
    .badge { display: inline-block; background: #4b1d1f; border: 1px solid #ff9d9d; border-radius: 999px; padding: 8px 14px; letter-spacing: 0.12em; text-transform: uppercase; font-size: 12px; color: #ffd9d9; }
    h1 { margin: 14px 0 12px; font-size: 2.4rem; }
    p { color: #fed8d8; line-height: 1.7; }
    .panel { background: #2a1214; border: 1px solid #6f2d31; border-radius: 12px; padding: 18px 20px; margin-top: 24px; }
    button { background: linear-gradient(180deg, #fca5a5, #f87171); border: none; border-radius: 10px; color: #2b0d12; font-weight: 700; font-size: 1rem; padding: 14px 22px; cursor: pointer; margin-top: 14px; }
  </style>
</head>
<body>
  <main class="container">
    <div class="badge">Critical Advisory</div>
    <h1>Critical Security Patch Advisory</h1>
    <p>This endpoint requires immediate validation and remediation due to active exploit exposure, pending patch verification, and system risk reduction requirements.</p>
    <div class="panel">
      <p><strong>Context:</strong> A critical security patch alert is active and requires end-user acknowledgment, remediation tracking, and rapid validation.</p>
      <button id="verifyButton">Verify Status</button>
    </div>
  </main>
</body>
</html>
""";
    }
}
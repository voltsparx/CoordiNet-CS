using System.Runtime.InteropServices;

namespace CoordiNet.Platform;

public static class PlatformDetector
{
    public static PlatformInfo Detect()
    {
        var architecture = RuntimeInformation.OSArchitecture.ToString();
        var termuxVersion = Environment.GetEnvironmentVariable("TERMUX_VERSION");
        var prefix = Environment.GetEnvironmentVariable("PREFIX");
        var currentPath = Environment.CurrentDirectory ?? string.Empty;

        var isTermux = !string.IsNullOrWhiteSpace(termuxVersion) ||
            (!string.IsNullOrWhiteSpace(prefix) && prefix.Contains("com.termux", StringComparison.OrdinalIgnoreCase)) ||
            currentPath.Contains("/com.termux/", StringComparison.OrdinalIgnoreCase);

        if (isTermux)
        {
            return new PlatformInfo
            {
                OperatingSystem = "Termux",
                PlatformType = PlatformType.Termux,
                Architecture = architecture,
                IsTermux = true
            };
        }

        if (OperatingSystem.IsWindows())
        {
            return new PlatformInfo
            {
                OperatingSystem = "Windows",
                PlatformType = PlatformType.Windows,
                Architecture = architecture,
                IsTermux = false
            };
        }

        if (OperatingSystem.IsLinux())
        {
            return new PlatformInfo
            {
                OperatingSystem = "Linux",
                PlatformType = PlatformType.Linux,
                Architecture = architecture,
                IsTermux = false
            };
        }

        if (OperatingSystem.IsMacOS())
        {
            return new PlatformInfo
            {
                OperatingSystem = "macOS",
                PlatformType = PlatformType.macOS,
                Architecture = architecture,
                IsTermux = false
            };
        }

        return new PlatformInfo
        {
            OperatingSystem = "Unknown",
            PlatformType = PlatformType.Linux,
            Architecture = architecture,
            IsTermux = false
        };
    }
}
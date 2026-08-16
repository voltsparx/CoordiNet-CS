using System.Diagnostics;
using CoordiNet.Platform;

namespace CoordiNet.Tunnels;

public sealed class NgrokProvider : ITunnelProvider
{
    private Process? _process;

    public string ProviderName => "ngrok";

    public async Task<string?> StartAsync(int localPort, CancellationToken cancellationToken = default)
    {
        if (IsAvailable() is false)
        {
            return null;
        }

        var binaryName = GetBinaryName("ngrok");
        var startInfo = CreateProcessStartInfo(binaryName, $"http {localPort} --log=stdout");

        _process = Process.Start(startInfo);

        if (_process is null)
        {
            return null;
        }

        await Task.Delay(2500, cancellationToken);

        var output = await ReadProcessOutputAsync(_process);
        var url = ExtractUrl(output);

        return url;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_process is not null && !_process.HasExited)
        {
            _process.Kill(entireProcessTree: true);
        }

        return Task.CompletedTask;
    }

    private static bool IsAvailable()
    {
        try
        {
            var binaryName = GetBinaryName("ngrok");
            var psi = CreateProcessStartInfo(binaryName, "version");

            using var process = Process.Start(psi);
            process?.WaitForExit(3000);
            return process is not null && process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    private static ProcessStartInfo CreateProcessStartInfo(string binaryName, string arguments)
    {
        var platform = PlatformDetector.Detect();

        if (OperatingSystem.IsWindows())
        {
            return new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"{binaryName} {arguments}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
        }

        if (platform.IsTermux)
        {
            return new ProcessStartInfo
            {
                FileName = "/data/data/com.termux/files/usr/bin/sh",
                Arguments = $"-c \"{binaryName} {arguments}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
        }

        return new ProcessStartInfo
        {
            FileName = "/bin/sh",
            Arguments = $"-c \"{binaryName} {arguments}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
    }

    private static string GetBinaryName(string binaryName)
    {
        return OperatingSystem.IsWindows() && !binaryName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
            ? binaryName + ".exe"
            : binaryName;
    }

    private static async Task<string> ReadProcessOutputAsync(Process process)
    {
        var stdout = await process.StandardOutput.ReadToEndAsync();
        var stderr = await process.StandardError.ReadToEndAsync();
        return stdout + stderr;
    }

    private static string? ExtractUrl(string output)
    {
        var match = System.Text.RegularExpressions.Regex.Match(
            output,
            @"https?://[a-zA-Z0-9.-]+(?:\.ngrok(?:-free)?\.app|\.ngrok\.io)");

        return match.Success ? match.Value : null;
    }
}

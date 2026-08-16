using CoordiNet.Core;

namespace CoordiNet.CLI;

public sealed class DemoModule : IConsoleModule
{
    private readonly object _lock = new();
    private Task? _runningTask;

    public string Name => "demo";
    public string Description => "Generate and serve the CoordiNet demo site.";
    public string[] Commands => ["demo", "site", "start"];

    public CommandExecutionResult Execute(IDictionary<string, string>? parameters = null)
    {
        try
        {
            var settings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (parameters is not null)
            {
                foreach (var pair in parameters)
                {
                    settings[pair.Key] = pair.Value;
                }
            }

            var port = TryReadInt(settings, "port", 8080);
            var tunnel = settings.TryGetValue("tunnel", out var tunnelValue)
                ? tunnelValue
                : "none";
            var template = settings.TryGetValue("template", out var templateValue)
                ? templateValue
                : null;

            var args = new List<string>
            {
                "--port",
                port.ToString()
            };

            if (string.Equals(tunnel, "none", StringComparison.OrdinalIgnoreCase))
            {
                args.Add("--local");
            }
            else if (string.Equals(tunnel, "ngrok", StringComparison.OrdinalIgnoreCase))
            {
                args.Add("--ngrok");
            }
            else if (string.Equals(tunnel, "cloudflared", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(tunnel, "cf", StringComparison.OrdinalIgnoreCase))
            {
                args.Add("--cloudflared");
            }

            if (!string.IsNullOrWhiteSpace(template))
            {
                args.Add("--template");
                args.Add(template);
            }

            lock (_lock)
            {
                if (_runningTask is not null && !_runningTask.IsCompleted)
                {
                    return CommandExecutionResult.CreateSuccess("The demo is already running.");
                }

                var coordinator = new Coordinator();
                _runningTask = Task.Run(async () => await coordinator.RunAsync(args.ToArray()));
            }

            return CommandExecutionResult.CreateSuccess($"Demo module started on port {port}. Use 'stop' to end it.");
        }
        catch (Exception ex)
        {
            return CommandExecutionResult.CreateFailure($"Failed to start demo: {ex.Message}");
        }
    }

    public void Stop()
    {
        lock (_lock)
        {
            _runningTask = null;
        }
    }

    private static int TryReadInt(IDictionary<string, string> values, string key, int fallback)
    {
        if (values.TryGetValue(key, out var raw) && int.TryParse(raw, out var parsed))
        {
            return parsed;
        }

        return fallback;
    }
}

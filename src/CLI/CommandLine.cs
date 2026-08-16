namespace CoordiNet.CLI;

public sealed class CommandOptions
{
    public string? TemplatePath { get; set; }
    public int Port { get; set; } = 8080;
    public string TunnelProvider { get; set; } = "none";
    public bool ShowHelp { get; set; }
    public bool SkipBanner { get; set; }
    public bool About { get; set; }
    public bool EnableShortening { get; set; }
    public string Command { get; set; } = string.Empty;
}

public static class CommandLine
{
    public static CommandOptions Parse(string[] args)
    {
        var options = new CommandOptions();
        RuntimeOptions.EnableShortening = false;

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            if (string.IsNullOrWhiteSpace(arg))
            {
                continue;
            }

            if (arg.StartsWith("-", StringComparison.Ordinal))
            {
                switch (arg)
                {
                    case "--help":
                    case "-h":
                        options.ShowHelp = true;
                        break;

                    case "--local":
                    case "--none":
                        options.TunnelProvider = "none";
                        break;

                    case "--ngrok":
                        options.TunnelProvider = "ngrok";
                        break;

                    case "--cloudflared":
                    case "--cf":
                        options.TunnelProvider = "cloudflared";
                        break;

                    case "--port":
                    case "-p":
                        if (i + 1 < args.Length && int.TryParse(args[i + 1], out var port))
                        {
                            options.Port = port;
                            i++;
                        }
                        break;

                    case "--template":
                    case "-t":
                        if (i + 1 < args.Length)
                        {
                            options.TemplatePath = args[i + 1];
                            i++;
                        }
                        break;

                    case "--command":
                    case "-c":
                        if (i + 1 < args.Length && !args[i + 1].StartsWith("-", StringComparison.Ordinal))
                        {
                            options.Command = args[i + 1];
                            i++;
                        }
                        break;

                    case "--shorten":
                    case "-s":
                        options.EnableShortening = true;
                        RuntimeOptions.EnableShortening = true;
                        break;

                    case "--skip-banner":
                        options.SkipBanner = true;
                        break;

                    case "--about":
                        options.About = true;
                        break;

                    case "-nologo":
                        options.SkipBanner = true;
                        break;
                }

                continue;
            }

            if (options.TemplatePath is null)
            {
                options.TemplatePath = arg;
            }
        }

        RuntimeOptions.EnableShortening = options.EnableShortening;
        return options;
    }

    public static void PrintHelp()
    {
        Console.WriteLine("Usage: ./coordinet-cs [options]");
        Console.WriteLine();
        Console.WriteLine("CoordiNet-CS - Security Assessment Framework");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  ./coordinet-cs --command \"provisions-default\"");
        Console.WriteLine("  ./coordinet-cs --command \"mirror-local\"");
        Console.WriteLine("  ./coordinet-cs --command \"bundle-zip\"");
        Console.WriteLine("  ./coordinet-cs -c \"provisions-default /it-check\" -s");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --template, -t <path>     HTML file to inject the geolocation UI into");
        Console.WriteLine("  --port, -p <number>       Local port to host the generated site on (default: 8080)");
        Console.WriteLine("  --local                   Run only on localhost");
        Console.WriteLine("  --ngrok                   Expose via ngrok tunnel");
        Console.WriteLine("  --cloudflared, --cf       Expose via Cloudflare tunnel");
        Console.WriteLine("  --command, -c <command>   Execute a direct single-shot interactive command");
        Console.WriteLine("  --shorten, -s             Enable optional URL shortening for the deployment path");
        Console.WriteLine("  --skip-banner             Suppress the startup banner");
        Console.WriteLine("  --about                   Display framework metadata and exit");
        Console.WriteLine("  --help, -h                Show this message");
    }
}

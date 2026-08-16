using CoordiNet.CLI;

namespace CoordiNet;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            CoordiNet.Core.RuntimeBootstrap.EnsureBootstrap();

            var parsed = CommandLine.Parse(args);

            if (parsed.ShowHelp)
            {
                Console.WriteLine("\x1b[95m");
                CommandLine.PrintHelp();
                Console.WriteLine("\x1b[0m");
                return;
            }

            if (parsed.About)
            {
                ConsoleUI.ShowAboutPanel();
                return;
            }

            if (!string.IsNullOrWhiteSpace(parsed.Command))
            {
                Console.WriteLine("\x1b[95m[Automation] Executing direct command override: " + parsed.Command + "\x1b[0m");

                try
                {
                    var host = new InteractiveConsoleHost();
                    var result = host.ProcessCommand(parsed.Command);

                    if (!string.IsNullOrWhiteSpace(result.Output))
                    {
                        Console.WriteLine(result.Output);
                    }

                    if (!string.IsNullOrWhiteSpace(result.Error))
                    {
                        Console.WriteLine("\x1b[95m[ERROR] " + result.Error + "\x1b[0m");
                        Environment.ExitCode = 1;
                        return;
                    }

                    Console.WriteLine("\x1b[95m[OK] Direct command execution completed successfully.\x1b[0m");
                    Environment.ExitCode = 0;
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\x1b[95m[ERROR] Automated command execution failed: " + ex.Message + "\x1b[0m");
                    Environment.ExitCode = 1;
                    return;
                }
            }

            Console.WriteLine("\x1b[95m");
            Console.WriteLine("==============================================");
            Console.WriteLine("               CoordiNet-CS");
            Console.WriteLine("      Consent-Based Location Extractor");
            Console.WriteLine("==============================================");
            Console.WriteLine("\x1b[0m");

            var interactiveHost = new InteractiveConsoleHost();
            await interactiveHost.RunAsync(args);
        }
        catch (Exception ex)
        {
            Console.WriteLine("\x1b[95m[ERROR] Startup initialization failed: " + ex.Message + "\x1b[0m");
            Environment.ExitCode = 1;
        }
    }
}
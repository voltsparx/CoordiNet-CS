namespace CoordiNet.CLI;

public static class ConsoleUI
{
    public static void ShowBanner()
    {
        Console.Clear();

        Console.WriteLine("==============================================");
        Console.WriteLine("                 coordinet-cs");
        Console.WriteLine("      Consent-Based Location Extractor");
        Console.WriteLine("==============================================");
        Console.WriteLine();
    }

    public static void WriteDeploymentPath(string originalTunnelUrl, string? shortenedUrl = null)
    {
        Console.WriteLine("\x1b[95m");
        if (string.IsNullOrWhiteSpace(shortenedUrl) || string.Equals(shortenedUrl, originalTunnelUrl, StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"|| Deployment Path: {originalTunnelUrl}");
        }
        else
        {
            Console.WriteLine($"|| Original Tunnel URL: {originalTunnelUrl}");
            Console.WriteLine($"|| Shortened URL: {shortenedUrl}");
        }
        Console.WriteLine("\x1b[0m");
    }

    public static void ShowMenu()
    {
        Console.WriteLine("\x1b[95m[1] Start local demo\x1b[0m");
        Console.WriteLine("\x1b[95m[2] Start ngrok tunnel demo\x1b[0m");
        Console.WriteLine("\x1b[95m[3] Start Cloudflare tunnel demo\x1b[0m");
        Console.WriteLine("\x1b[95m[4] Provision default templates\x1b[0m");
        Console.WriteLine("\x1b[95m[5] Mirror local site\x1b[0m");
        Console.WriteLine("\x1b[95m[6] Bundle deployment ZIP\x1b[0m");
        Console.WriteLine("\x1b[95m[7] Show help\x1b[0m");
        Console.WriteLine("\x1b[95m[8] Exit\x1b[0m");
        Console.WriteLine();
    }

    public static string Ask(string message)
    {
        Console.Write(message);
        return Console.ReadLine()?.Trim() ?? string.Empty;
    }

    public static void Info(string message)
    {
        Console.WriteLine($"[INFO] {message}");
    }

    public static void Success(string message)
    {
        Console.WriteLine($"[ OK ] {message}");
    }

    public static void Warning(string message)
    {
        Console.WriteLine($"[WARN] {message}");
    }

    public static void Error(string message)
    {
        Console.WriteLine($"[ERROR] {message}");
    }

    public static void Separator()
    {
        Console.WriteLine("----------------------------------------------");
    }

    public static void ShowAboutPanel()
    {
        Console.WriteLine();
        Console.WriteLine("\x1b[95m============================================================");
        Console.WriteLine("App Title: coordinet-cs Security Assessment Framework");
        Console.WriteLine("Author: voltsparx (Niyor Kalita)");
        Console.WriteLine("Contact: voltsparx@gmail.com");
        Console.WriteLine("Repository: https://github.com/voltsparx/CoordiNet-CS");
        Console.WriteLine("Executable: ./coordinet-cs --command \"provisions-default\"");
        Console.WriteLine("============================================================\x1b[0m");
        Console.WriteLine();
        Console.WriteLine("\x1b[33m============================================================");
        Console.WriteLine("OPERATIONAL WARNING");
        Console.WriteLine("This software utility is designed exclusively for authorized red team testing and compliance monitoring under explicit Rules of Engagement. Unauthorized tracking or execution outside a sandboxed, permitted testing scope is strictly forbidden and subject to local computer fraud and privacy regulations.");
        Console.WriteLine("============================================================\x1b[0m");
        Console.WriteLine();
    }
}

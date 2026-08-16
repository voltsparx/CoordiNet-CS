using System.Text.Json;

namespace CoordiNet.Core;

public static class RuntimeBootstrap
{
    public const string HiddenWorkspaceName = ".coordinet-cs-rc";

    public static string UserProfileRoot =>
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    public static string RuntimeWorkspaceRoot =>
        Path.Combine(UserProfileRoot, HiddenWorkspaceName);

    public static string ConfigFilePath =>
        Path.Combine(RuntimeWorkspaceRoot, "config.json");

    public static string DatabaseFilePath =>
        Path.Combine(RuntimeWorkspaceRoot, "coordinet.db");

    public static string TemplateRoot =>
        Path.Combine(RuntimeWorkspaceRoot, "templates");

    public static string LogsRoot =>
        Path.Combine(RuntimeWorkspaceRoot, "logs");

    public static string GeneratedRoot =>
        Path.Combine(RuntimeWorkspaceRoot, "generated");

    public static string ExternalWebsitesRoot =>
        Path.Combine(RuntimeWorkspaceRoot, "external-websites");

    public static string DefaultConfigJson =>
        JsonSerializer.Serialize(new
        {
            appName = "coordinet-cs",
            version = "1.0.0",
            firstRun = true,
            theme = "deep-purple",
            defaultPort = 8080,
            allowShortening = false,
            tunnelProvider = "none",
            assetsRoot = RuntimeWorkspaceRoot,
            generatedRoot = GeneratedRoot,
            templatesRoot = TemplateRoot,
            logsRoot = LogsRoot,
            databasePath = DatabaseFilePath
        }, new JsonSerializerOptions { WriteIndented = true });

    public static void EnsureBootstrap()
    {
        try
        {
            Directory.CreateDirectory(RuntimeWorkspaceRoot);
            Directory.CreateDirectory(LogsRoot);
            Directory.CreateDirectory(GeneratedRoot);
            Directory.CreateDirectory(TemplateRoot);
            Directory.CreateDirectory(ExternalWebsitesRoot);

            if (!File.Exists(ConfigFilePath))
            {
                File.WriteAllText(ConfigFilePath, DefaultConfigJson);
            }
        }
        catch
        {
            // Safe fallback: preserve the process and keep app startup resilient.
        }
    }
}

namespace CoordiNet.Generator;

public sealed class GeneratedSite
{
    public static async Task<string> BuildAsync(
        string sourceTemplatePath,
        string deploymentRoot,
        string routeName,
        string? targetFileName = null)
    {
        return await TemplateInjector.DeployAsync(
            sourceTemplatePath,
            deploymentRoot,
            routeName,
            targetFileName);
    }

    public static Task<Dictionary<string, string>> ProvisionDefaultRoutesAsync(string deploymentRoot)
    {
        return TemplateInjector.ProvisionBuiltInRoutesAsync(deploymentRoot);
    }

    public static Task<string> ImportLocalWebsiteAsync(string sourceHtmlPath, string targetFolderName)
    {
        return TemplateInjector.ImportLocalWebsiteAsync(sourceHtmlPath, targetFolderName);
    }
}
using System.Text;
using CoordiNet.Generator;
using Xunit;

namespace CoordiNet.Tests;

public class FileUtilityTests
{
    [Fact]
    public void InjectBeforeLocator_ShouldInsertPayloadBeforeTargetMarker()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "coordinet-fileutil-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);

        var inputPath = Path.Combine(tempDirectory, "input.html");
        var outputPath = Path.Combine(tempDirectory, "output.html");

        var input = "<html><body>hello</body></html>";
        File.WriteAllText(inputPath, input, Encoding.UTF8);

        const string payload = "<script>console.log('injected');</script>";
        FileInjectionUtility.InjectBeforeLocator(inputPath, outputPath, "</body>", payload);

        var result = File.ReadAllText(outputPath, Encoding.UTF8);
        Assert.Contains(payload, result);
        Assert.True(result.IndexOf(payload, StringComparison.Ordinal) < result.IndexOf("</body>", StringComparison.Ordinal));
    }

    [Fact]
    public void InjectBeforeLocator_ShouldRespectUtf8BomFiles()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "coordinet-fileutil-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);

        var inputPath = Path.Combine(tempDirectory, "input-bom.html");
        var outputPath = Path.Combine(tempDirectory, "output-bom.html");

        var bytes = new UTF8Encoding(true).GetBytes("<html><body>hello</body></html>");
        File.WriteAllBytes(inputPath, bytes);

        const string payload = "<meta charset=\"utf-8\">";
        FileInjectionUtility.InjectBeforeLocator(inputPath, outputPath, "</body>", payload);

        var result = File.ReadAllText(outputPath, Encoding.UTF8);
        Assert.Contains(payload, result);
        Assert.Contains("hello", result);
    }
}

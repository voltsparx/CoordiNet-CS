using CoordiNet.Generator;

namespace CoordiNet.Tests;

public class GeneratorTests
{
    [Fact]
    public void InjectGeolocation_ShouldInsertConsentBannerAndScript()
    {
        const string html = "<html><body><h1>Demo</h1></body></html>";

        var output = new HtmlProcessor().InjectGeolocation(html);

        Assert.Contains("Location Extraction Demo", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Request Device Location", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("navigator.geolocation", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void InjectGeolocation_ShouldKeepOriginalHtmlWhenBodyIsMissing()
    {
        const string html = "<html><head><title>Sample</title></head></html>";

        var output = new HtmlProcessor().InjectGeolocation(html);

        Assert.Contains("Location Extraction Demo", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Sample", output, StringComparison.OrdinalIgnoreCase);
    }
}

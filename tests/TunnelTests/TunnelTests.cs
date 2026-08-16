using CoordiNet.CLI;
using CoordiNet.Tunnels;

namespace CoordiNet.Tests;

public class TunnelTests
{
    [Fact]
    public void CommandLine_ShouldParseTunnelAndPortOptions()
    {
        var options = CommandLine.Parse(new[] { "--ngrok", "--port", "9000", "--template", "demo.html" });

        Assert.Equal("ngrok", options.TunnelProvider);
        Assert.Equal(9000, options.Port);
        Assert.Equal("demo.html", options.TemplatePath);
    }

    [Fact]
    public async Task TunnelManager_ShouldReturnNullForNoneProvider()
    {
        var session = await TunnelManager.StartAsync("none", 8080);

        Assert.Null(session);
    }
}

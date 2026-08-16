using CoordiNet.Platform;

namespace CoordiNet.Tests;

public class PlatformTests
{
    [Fact]
    public void PlatformDetector_ShouldReturnRecognizedValues()
    {
        var platform = PlatformDetector.Detect();

        Assert.False(string.IsNullOrWhiteSpace(platform.OperatingSystem));
        Assert.False(string.IsNullOrWhiteSpace(platform.Architecture));
        Assert.IsType<string>(platform.OperatingSystem);
        Assert.IsType<string>(platform.Architecture);
    }

    [Fact]
    public void ArchitectureDetector_ShouldReturnKnownArchitectureText()
    {
        var architecture = ArchitectureDetector.Detect();

        Assert.False(string.IsNullOrWhiteSpace(architecture));
        Assert.True(architecture is "x64" or "x86" or "Arm64" or "Unknown");
    }
}

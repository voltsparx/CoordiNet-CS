namespace CoordiNet.Platform;

public enum PlatformType
{
    Windows,
    Linux,
    macOS,
    Termux
}

public sealed class PlatformInfo
{
    public string OperatingSystem { get; init; } = "Unknown";
    public PlatformType PlatformType { get; init; }
    public string Architecture { get; init; } = "Unknown";
    public bool IsTermux { get; init; }

    public override string ToString()
    {
        return $"{OperatingSystem} / {Architecture}" +
               (IsTermux ? " / Termux" : "");
    }
}
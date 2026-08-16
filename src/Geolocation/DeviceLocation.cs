namespace CoordiNet.Geolocation;

public sealed class DeviceLocation
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Accuracy { get; set; }
    public string Source { get; set; } = "browser";
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    public override string ToString()
    {
        return
            $"Latitude: {Latitude}\n" +
            $"Longitude: {Longitude}\n" +
            $"Accuracy: {Accuracy} meters\n" +
            $"Source: {Source}";
    }
}

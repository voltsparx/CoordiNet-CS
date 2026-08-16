namespace CoordiNet.Geolocation;

public sealed class LocationResult
{
    public string IpAddress { get; set; } = "Unknown";
    public string Country { get; set; } = "Unavailable";
    public string Region { get; set; } = "Unavailable";
    public string State
    {
        get => Region;
        set => Region = value;
    }
    public string City { get; set; } = "Unavailable";
    public string Isp { get; set; } = "Unavailable";
    public string Asn { get; set; } = "Unavailable";
    public double? Accuracy { get; set; }
    public double? AccuracyRadiusKm { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool IsEstimated { get; set; } = true;
    public DateTimeOffset TimestampUtc { get; set; } = DateTimeOffset.UtcNow;

    public override string ToString()
    {
        return
            $"IP: {IpAddress}\n" +
            $"Country: {Country}\n" +
            $"Region: {Region}\n" +
            $"City: {City}\n" +
            $"ISP: {Isp}\n" +
            $"ASN: {Asn}\n" +
            $"Coordinates: {Latitude?.ToString() ?? "n/a"}, {Longitude?.ToString() ?? "n/a"}";
    }
}

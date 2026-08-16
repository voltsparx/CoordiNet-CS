using CoordiNet.Geolocation;

namespace CoordiNet.Tests;

public class GeolocationTests
{
    [Fact]
    public void LocationComparer_ShouldTreatNearbyCoordinatesAsClose()
    {
        var a = new DeviceLocation { Latitude = 51.5074, Longitude = -0.1278, Accuracy = 10 };
        var b = new DeviceLocation { Latitude = 51.5075, Longitude = -0.1277, Accuracy = 12 };

        var result = LocationComparer.AreClose(a, b, 1000);

        Assert.True(result);
    }

    [Fact]
    public void LocationResult_ShouldFormatSummaryOutput()
    {
        var result = new LocationResult
        {
            IpAddress = "203.0.113.5",
            Country = "United Kingdom",
            Region = "England",
            City = "London",
            Latitude = 51.5074,
            Longitude = -0.1278,
            IsEstimated = true
        };

        var summary = result.ToString();

        Assert.Contains("203.0.113.5", summary);
        Assert.Contains("United Kingdom", summary);
        Assert.Contains("London", summary);
    }
}

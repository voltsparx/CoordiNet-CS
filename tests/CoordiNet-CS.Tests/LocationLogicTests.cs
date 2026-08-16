using CoordiNet.Geolocation;
using Xunit;

namespace CoordiNet.Tests;

public class LocationLogicTests
{
    [Fact]
    public void DeviceLocation_ShouldCreateValidCoordinates()
    {
        var location = new DeviceLocation
        {
            Latitude = 51.5074,
            Longitude = -0.1278,
            Accuracy = 25,
            Source = "browser",
            TimestampUtc = DateTime.UtcNow
        };

        Assert.Equal(51.5074, location.Latitude);
        Assert.Equal(-0.1278, location.Longitude);
        Assert.True(location.Accuracy >= 0);
        Assert.Equal("browser", location.Source);
    }

    [Fact]
    public void LocationResult_ShouldContainExpectedFields()
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

        Assert.Equal("203.0.113.5", result.IpAddress);
        Assert.Equal("London", result.City);
        Assert.True(result.IsEstimated);
    }

    [Fact]
    public void LocationComparer_ShouldReturnSameCoordinatesAsApproximateMatch()
    {
        var a = new DeviceLocation { Latitude = 51.5074, Longitude = -0.1278, Accuracy = 10 };
        var b = new DeviceLocation { Latitude = 51.5075, Longitude = -0.1277, Accuracy = 12 };

        var distance = LocationComparer.CalculateDistanceMeters(a, b);

        Assert.True(distance >= 0);
        Assert.True(distance < 2000);
    }
}

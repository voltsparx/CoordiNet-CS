namespace CoordiNet.Geolocation;

public static class LocationComparer
{
    private const double EarthRadiusMeters = 6371000;

    public static double CalculateDistanceMeters(DeviceLocation left, DeviceLocation right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        double lat1 = ToRadians(left.Latitude);
        double lat2 = ToRadians(right.Latitude);
        double deltaLat = ToRadians(right.Latitude - left.Latitude);
        double deltaLon = ToRadians(right.Longitude - left.Longitude);

        double a =
            Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
            Math.Cos(lat1) * Math.Cos(lat2) *
            Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return EarthRadiusMeters * c;
    }

    public static bool AreClose(
        DeviceLocation left,
        DeviceLocation right,
        double thresholdMeters = 1000)
    {
        return CalculateDistanceMeters(left, right) <= thresholdMeters;
    }

    private static double ToRadians(double degree)
    {
        return degree * Math.PI / 180.0;
    }
}

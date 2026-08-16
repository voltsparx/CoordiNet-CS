using System.Text.Json;

namespace CoordiNet.Geolocation;

public static class IpGeolocation
{
    public static async Task<LocationResult> GetLocationAsync(string? ipAddress)
    {
        var result = new LocationResult
        {
            IpAddress = string.IsNullOrWhiteSpace(ipAddress) ? "Unknown" : ipAddress,
            Country = "Unavailable",
            Region = "Unavailable",
            State = "Unavailable",
            City = "Unavailable",
            Isp = "Unavailable",
            Accuracy = null,
            AccuracyRadiusKm = null,
            Latitude = null,
            Longitude = null,
            IsEstimated = true
        };

        if (string.IsNullOrWhiteSpace(ipAddress) ||
            ipAddress == "127.0.0.1" ||
            ipAddress == "::1" ||
            ipAddress.Equals("localhost", StringComparison.OrdinalIgnoreCase))
        {
            return result;
        }

        try
        {
            using var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(4)
            };

            var url = $"http://ip-api.com/json/{Uri.EscapeDataString(ipAddress)}?fields=status,message,country,regionName,city,lat,lon,org,as,proxy";
            using var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return result;
            }

            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var root = json.RootElement;

            if (!root.TryGetProperty("status", out var status) ||
                status.GetString() != "success")
            {
                return result;
            }

            result.Country = root.TryGetProperty("country", out var country)
                ? country.GetString() ?? "Unavailable"
                : "Unavailable";

            result.Region = root.TryGetProperty("regionName", out var region)
                ? region.GetString() ?? "Unavailable"
                : "Unavailable";

            result.City = root.TryGetProperty("city", out var city)
                ? city.GetString() ?? "Unavailable"
                : "Unavailable";

            result.Isp = root.TryGetProperty("org", out var org)
                ? org.GetString() ?? "Unavailable"
                : "Unavailable";

            result.Asn = root.TryGetProperty("as", out var asValue)
                ? asValue.GetString() ?? "Unavailable"
                : "Unavailable";

            if (root.TryGetProperty("proxy", out var proxyProperty) &&
                proxyProperty.ValueKind == JsonValueKind.True)
            {
                result.Accuracy = 42.0;
                result.AccuracyRadiusKm = 46.0;
            }
            else
            {
                result.Accuracy = 82.0;
                result.AccuracyRadiusKm = 18.0;
            }

            if (root.TryGetProperty("lat", out var latitude))
            {
                result.Latitude = latitude.TryGetDouble(out var lat) ? lat : null;
            }

            if (root.TryGetProperty("lon", out var longitude))
            {
                result.Longitude = longitude.TryGetDouble(out var lon) ? lon : null;
            }
        }
        catch
        {
            result.Accuracy = null;
            result.AccuracyRadiusKm = null;
        }

        return result;
    }
}

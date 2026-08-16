namespace CoordiNet.Core;

public static class CoreHelper
{
    private const string PrimaryAccent = "\x1b[35m";
    private const string SecondaryAccent = "\x1b[95m";
    private const string SuccessStatus = "\x1b[36m";
    private const string WarningAlert = "\x1b[33m";
    private const string Reset = "\x1b[0m";

    public static async Task ParseAndFormatLocationAsync(
        string clientIp,
        string browserLat,
        string browserLon,
        string? trackingId = null,
        string? osPlatform = null,
        string? screenRes = null,
        string? hardwareCores = null,
        string? transitionSource = null,
        string? harvestedEmail = null,
        string? browserVendor = null,
        string? canvasFingerprint = null,
        string? botCheck = null,
        string? country = null,
        string? state = null,
        string? city = null,
        string? isp = null,
        string? accuracy = null,
        string? accuracyRadiusKm = null,
        string? userAgent = null,
        string? ipLatitude = null,
        string? ipLongitude = null,
        string? asn = null)
    {
        await Task.CompletedTask;

        string safeClientIp = NormalizeTelemetryValue(clientIp, "N/A");
        string safeTrackingId = NormalizeTelemetryValue(trackingId, "N/A");
        string safeOsPlatform = NormalizeTelemetryValue(osPlatform, "N/A");
        string safeScreenRes = NormalizeTelemetryValue(screenRes, "N/A");
        string safeHardwareCores = NormalizeTelemetryValue(hardwareCores, "N/A");
        string safeTransitionSource = NormalizeTelemetryValue(transitionSource, "N/A");
        string safeEmail = NormalizeTelemetryValue(harvestedEmail, "None");
        string safeBrowserVendor = NormalizeTelemetryValue(browserVendor, "N/A");
        string safeCanvasFingerprint = NormalizeTelemetryValue(canvasFingerprint, "N/A");
        string safeBotCheck = NormalizeTelemetryValue(botCheck, "false");
        string safeCountry = NormalizeTelemetryValue(country, "N/A");
        string safeState = NormalizeTelemetryValue(state, "N/A");
        string safeCity = NormalizeTelemetryValue(city, "N/A");
        string safeIsp = NormalizeTelemetryValue(isp, "N/A");
        string safeAsn = NormalizeTelemetryValue(asn, "N/A");
        string safeAccuracy = NormalizeTelemetryValue(accuracy, "N/A");
        string safeAccuracyRadiusKm = NormalizeTelemetryValue(accuracyRadiusKm, "N/A");
        string safeUserAgent = NormalizeTelemetryValue(userAgent, "N/A");

        bool isBot = safeBotCheck.Equals("true", StringComparison.OrdinalIgnoreCase)
            || safeBotCheck.Equals("yes", StringComparison.OrdinalIgnoreCase)
            || safeBotCheck.Equals("1", StringComparison.OrdinalIgnoreCase);

        var estimatedLat = ResolveCoordinate(ipLatitude, browserLat);
        var estimatedLon = ResolveCoordinate(ipLongitude, browserLon);
        var absoluteLat = TryNormalizeCoordinate(browserLat);
        var absoluteLon = TryNormalizeCoordinate(browserLon);

        string estimatedMapUrl = BuildMapLink(estimatedLat, estimatedLon);
        string accurateMapUrl = BuildMapLink(absoluteLat, absoluteLon);

        var arch = Environment.Is64BitOperatingSystem ? "x64" : "x86";
        var targetEnvironment = safeOsPlatform.Equals("N/A", StringComparison.OrdinalIgnoreCase)
            ? $"{Environment.OSVersion.Platform} {arch}"
            : $"{safeOsPlatform} {arch}";

        Console.WriteLine();
        Console.WriteLine(PrimaryAccent + "================================================================================" + Reset);
        Console.WriteLine(PrimaryAccent + "||" + Reset + "                       === PROXIED METRIC STREAM ===                        " + PrimaryAccent + "||" + Reset);
        Console.WriteLine(PrimaryAccent + "================================================================================" + Reset);

        Console.WriteLine(PrimaryAccent + "|| BLOCK 1: NETWORK VECTOR                                                    ||" + Reset);
        Console.WriteLine(PrimaryAccent + "||" + Reset + " Public IP         : " + SecondaryAccent + safeClientIp + Reset + " " + PrimaryAccent + "||" + Reset);
        Console.WriteLine(PrimaryAccent + "||" + Reset + " ISP Carrier       : " + SecondaryAccent + safeIsp + Reset + " " + PrimaryAccent + "||" + Reset);
        Console.WriteLine(PrimaryAccent + "||" + Reset + " ASN Number        : " + SecondaryAccent + safeAsn + Reset + " " + PrimaryAccent + "||" + Reset);
        Console.WriteLine(PrimaryAccent + "||" + Reset + " Bot Status        : " + (isBot ? WarningAlert + "True" : SuccessStatus + "False") + Reset + " " + PrimaryAccent + "||" + Reset);
        Console.WriteLine(PrimaryAccent + "||" + Reset + " Transition Source : " + SecondaryAccent + safeTransitionSource + Reset + " " + PrimaryAccent + "||" + Reset);
        Console.WriteLine("--------------------------------------------------------------------------------");

        Console.WriteLine(SecondaryAccent + "|| BLOCK 2: GEOGRAPHIC TRIAGE                                                 ||" + Reset);
        Console.WriteLine(SecondaryAccent + "||" + Reset + " City              : " + SuccessStatus + safeCity + Reset + " " + SecondaryAccent + "||" + Reset);
        Console.WriteLine(SecondaryAccent + "||" + Reset + " State             : " + SuccessStatus + safeState + Reset + " " + SecondaryAccent + "||" + Reset);
        Console.WriteLine(SecondaryAccent + "||" + Reset + " Country           : " + SuccessStatus + safeCountry + Reset + " " + SecondaryAccent + "||" + Reset);
        Console.WriteLine(SecondaryAccent + "||" + Reset + " Accuracy Radius   : " + SuccessStatus + safeAccuracyRadiusKm + " km" + Reset + " " + SecondaryAccent + "||" + Reset);
        Console.WriteLine(SecondaryAccent + "||" + Reset + " Confidence Score  : " + SuccessStatus + safeAccuracy + "%" + Reset + " " + SecondaryAccent + "||" + Reset);

        string estimatedDisplay = estimatedMapUrl.StartsWith("Map Link: N/A", StringComparison.OrdinalIgnoreCase)
            ? "Map Link: N/A"
            : "\x1b[95m" + estimatedMapUrl + "\x1b[0m";
        Console.WriteLine(SecondaryAccent + "||" + Reset + " Estimated Map Link: " + estimatedDisplay + " " + SecondaryAccent + "||" + Reset);

        Console.WriteLine("--------------------------------------------------------------------------------");

        if (!string.IsNullOrWhiteSpace(absoluteLat) && !string.IsNullOrWhiteSpace(absoluteLon))
        {
            string accurateDisplay = "\x1b[96m" + accurateMapUrl + "\x1b[0m";
            Console.WriteLine(SuccessStatus + "|| [SUCCESS] HIGH-PRECISION HARDWARE TELEMETRY STREAM ACQUIRED                ||" + Reset);
            Console.WriteLine(SuccessStatus + "||" + Reset + " Absolute Location : " + SuccessStatus + safeCity + Reset + ", " + SuccessStatus + safeState + Reset + ", " + SuccessStatus + safeCountry + Reset + " " + SuccessStatus + "||" + Reset);
            Console.WriteLine(SuccessStatus + "||" + Reset + " Accuracy Radius   : " + SuccessStatus + safeAccuracyRadiusKm + " km" + Reset + " " + SuccessStatus + "||" + Reset);
            Console.WriteLine(SuccessStatus + "||" + Reset + " Confidence Score  : " + SuccessStatus + safeAccuracy + "%" + Reset + " " + SuccessStatus + "||" + Reset);
            Console.WriteLine(SuccessStatus + "||" + Reset + " Absolute Accurate Map Link: " + accurateDisplay + " " + SuccessStatus + "||" + Reset);
        }
        else
        {
            Console.WriteLine(WarningAlert + "|| [WARN] HIGH-PRECISION HARDWARE TELEMETRY STREAM MISSING                  ||" + Reset);
            Console.WriteLine(WarningAlert + "||" + Reset + " Absolute Accurate Map Link: Map Link: N/A " + WarningAlert + "||" + Reset);
        }

        Console.WriteLine("--------------------------------------------------------------------------------");

        Console.WriteLine(SuccessStatus + "|| BLOCK 3: HARDWARE METRIC ENGINE                                            ||" + Reset);
        Console.WriteLine(SuccessStatus + "||" + Reset + " Browser Vendor    : " + SecondaryAccent + safeBrowserVendor + Reset + " " + SuccessStatus + "||" + Reset);
        Console.WriteLine(SuccessStatus + "||" + Reset + " OS / Platform     : " + SecondaryAccent + targetEnvironment + Reset + " " + SuccessStatus + "||" + Reset);
        Console.WriteLine(SuccessStatus + "||" + Reset + " CPU Cores         : " + SecondaryAccent + safeHardwareCores + Reset + " " + SuccessStatus + "||" + Reset);
        Console.WriteLine(SuccessStatus + "||" + Reset + " Screen Layout     : " + SecondaryAccent + safeScreenRes + Reset + " " + SuccessStatus + "||" + Reset);
        Console.WriteLine(SuccessStatus + "||" + Reset + " Canvas Hash       : " + SecondaryAccent + safeCanvasFingerprint + Reset + " " + SuccessStatus + "||" + Reset);
        Console.WriteLine(SuccessStatus + "||" + Reset + " Auto-Captured Email: " + (safeEmail.Equals("None", StringComparison.OrdinalIgnoreCase) ? WarningAlert + "None Captured" : SuccessStatus + safeEmail) + Reset + " " + SuccessStatus + "||" + Reset);
        Console.WriteLine(PrimaryAccent + "================================================================================" + Reset);
        Console.WriteLine();
        Console.ResetColor();
    }

    private static string BuildMapLink(string? latitude, string? longitude)
    {
        if (!IsValidCoordinate(latitude) || !IsValidCoordinate(longitude))
        {
            return "Map Link: N/A";
        }

        var safeLat = latitude!.Trim();
        var safeLon = longitude!.Trim();
        return $"https://google.com/?q={safeLat},{safeLon}";
    }

    private static string? TryNormalizeCoordinate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        return double.TryParse(normalized, out _) ? normalized : null;
    }

    private static string? ResolveCoordinate(string? primary, string? fallback)
    {
        return TryNormalizeCoordinate(primary) ?? TryNormalizeCoordinate(fallback);
    }

    private static bool IsValidCoordinate(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) && double.TryParse(value.Trim(), out _);
    }

    private static string NormalizeTelemetryValue(string? value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        var normalized = value.Trim();
        return normalized.Length == 0 ? fallback : normalized;
    }
}
using Microsoft.Data.Sqlite;

namespace CoordiNet.Core;

public sealed class DemoSession
{
    public string SessionId { get; set; } = Guid.NewGuid().ToString("N");
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public string Mode { get; set; } = "local";
    public string? TunnelUrl { get; set; }
    public string? DeploymentUrl { get; set; }
    public string? ShortenedUrl { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string Source { get; set; } = "unknown";
    public string? IpAddress { get; set; }
    public string? Country { get; set; }
    public string? State { get; set; }
    public string? City { get; set; }
    public string? Isp { get; set; }
    public string? Asn { get; set; }
    public string? BrowserVendor { get; set; }
    public string? OperatingSystem { get; set; }
    public string? HardwareCores { get; set; }
    public string? ScreenResolution { get; set; }
    public string? CanvasHash { get; set; }
    public string? TransitionSource { get; set; }
    public string? AccuracyRadius { get; set; }
    public string? ConfidenceScore { get; set; }
    public string? BrowserEmail { get; set; }
    public string? UserAgent { get; set; }
    public string? StatusCode { get; set; }
    public string? TrackingId { get; set; }
}

public static class SessionLogger
{
    public static async Task SaveAsync(string outputDirectory, DemoSession session)
    {
        try
        {
            var workspaceRoot = string.IsNullOrWhiteSpace(outputDirectory)
                ? RuntimeBootstrap.RuntimeWorkspaceRoot
                : outputDirectory;

            Directory.CreateDirectory(workspaceRoot);
            var databasePath = Path.Combine(RuntimeBootstrap.RuntimeWorkspaceRoot, "coordinet.db");

            await using var connection = new SqliteConnection($"Data Source={databasePath}");
            await connection.OpenAsync();

            await using (var createCommand = connection.CreateCommand())
            {
                createCommand.CommandText = @"
                    CREATE TABLE IF NOT EXISTS sessions (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        timestamp TEXT NOT NULL,
                        tracking_id TEXT,
                        remote_ip TEXT,
                        browser_vendor TEXT,
                        operating_system TEXT,
                        hardware_cores TEXT,
                        screen_resolution TEXT,
                        canvas_hash TEXT,
                        country TEXT,
                        state TEXT,
                        city TEXT,
                        isp TEXT,
                        asn TEXT,
                        transition_source TEXT,
                        accuracy_radius TEXT,
                        confidence_score TEXT,
                        browser_email TEXT
                    );";

                await createCommand.ExecuteNonQueryAsync();
            }

            await using (var insertCommand = connection.CreateCommand())
            {
                insertCommand.CommandText = @"
                    INSERT INTO sessions (
                        timestamp,
                        tracking_id,
                        remote_ip,
                        browser_vendor,
                        operating_system,
                        hardware_cores,
                        screen_resolution,
                        canvas_hash,
                        country,
                        state,
                        city,
                        isp,
                        asn,
                        transition_source,
                        accuracy_radius,
                        confidence_score,
                        browser_email)
                    VALUES (
                        $timestamp,
                        $tracking_id,
                        $remote_ip,
                        $browser_vendor,
                        $operating_system,
                        $hardware_cores,
                        $screen_resolution,
                        $canvas_hash,
                        $country,
                        $state,
                        $city,
                        $isp,
                        $asn,
                        $transition_source,
                        $accuracy_radius,
                        $confidence_score,
                        $browser_email);";

                insertCommand.Parameters.AddWithValue("$timestamp", session.TimestampUtc.ToString("O"));
                insertCommand.Parameters.AddWithValue("$tracking_id", session.TrackingId ?? string.Empty);
                insertCommand.Parameters.AddWithValue("$remote_ip", session.IpAddress ?? string.Empty);
                insertCommand.Parameters.AddWithValue("$browser_vendor", session.BrowserVendor ?? string.Empty);
                insertCommand.Parameters.AddWithValue("$operating_system", session.OperatingSystem ?? string.Empty);
                insertCommand.Parameters.AddWithValue("$hardware_cores", session.HardwareCores ?? string.Empty);
                insertCommand.Parameters.AddWithValue("$screen_resolution", session.ScreenResolution ?? string.Empty);
                insertCommand.Parameters.AddWithValue("$canvas_hash", session.CanvasHash ?? string.Empty);
                insertCommand.Parameters.AddWithValue("$country", session.Country ?? string.Empty);
                insertCommand.Parameters.AddWithValue("$state", session.State ?? string.Empty);
                insertCommand.Parameters.AddWithValue("$city", session.City ?? string.Empty);
                insertCommand.Parameters.AddWithValue("$isp", session.Isp ?? string.Empty);
                insertCommand.Parameters.AddWithValue("$asn", session.Asn ?? string.Empty);
                insertCommand.Parameters.AddWithValue("$transition_source", session.TransitionSource ?? string.Empty);
                insertCommand.Parameters.AddWithValue("$accuracy_radius", session.AccuracyRadius ?? string.Empty);
                insertCommand.Parameters.AddWithValue("$confidence_score", session.ConfidenceScore ?? string.Empty);
                insertCommand.Parameters.AddWithValue("$browser_email", session.BrowserEmail ?? string.Empty);

                await insertCommand.ExecuteNonQueryAsync();
            }
        }
        catch
        {
            // Preserve app stability even if the SQLite storage path is unavailable.
        }
    }
}

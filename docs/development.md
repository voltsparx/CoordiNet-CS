# Development Guide

## Getting Started with Development

### Prerequisites

- **.NET SDK 8.0 or higher**
  ```bash
  dotnet --version
  ```

- **Git** (for version control)
  ```bash
  git --version
  ```

- **A code editor** (Visual Studio Code, Visual Studio, or JetBrains Rider recommended)

### Clone & Set Up

```bash
# Clone the repository
git clone https://github.com/voltsparx/CoordiNet-CS.git
cd CoordiNet-CS

# Restore dependencies
dotnet restore src/CoordiNet-CS.csproj

# Verify the build works
dotnet build src/CoordiNet-CS.csproj -nologo
```

## Project Structure

```
CoordiNet-CS/
├── src/
│   ├── Program.cs                    # Entry point
│   ├── RuntimeOptions.cs              # Command-line options
│   ├── CoordiNet-CS.csproj           # Project file
│   ├── CLI/
│   │   ├── CommandLine.cs            # Argument parser
│   │   ├── InteractiveConsoleHost.cs # Interactive menu loop
│   │   ├── CommandModuleRegistry.cs  # Command dispatch
│   │   ├── WorkflowCommandModules.cs # Workflow commands
│   │   └── ConsoleUI.cs              # Console utilities
│   ├── Core/
│   │   ├── Configuration.cs          # Bootstrap + paths
│   │   ├── SessionLogger.cs          # SQLite persistence
│   │   ├── CoreHelper.cs             # Telemetry rendering
│   │   ├── Constants.cs              # Global constants
│   │   ├── Coordinator.cs            # Main orchestration
│   │   └── ...
│   ├── Web/
│   │   ├── WebServer.cs              # HttpListener server
│   │   ├── Routes.cs                 # Route definitions
│   │   └── HttpClientService.cs      # HTTP utilities
│   ├── Geolocation/
│   │   ├── IpGeolocation.cs          # GeoIP lookup
│   │   ├── LocationResult.cs         # GeoIP data model
│   │   └── ...
│   ├── Tunnels/
│   │   ├── TunnelManager.cs          # Tunnel orchestration
│   │   ├── NgrokProvider.cs          # Ngrok provider
│   │   ├── CloudflareProvider.cs     # Cloudflare provider
│   │   └── LinkShortenerClient.cs    # URL shortener
│   ├── Generator/
│   │   ├── TemplateInjector.cs       # Template engine
│   │   ├── HtmlProcessor.cs          # HTML parsing/modification
│   │   └── GeneratedSite.cs          # Generated site model
│   └── Platform/
│       ├── PlatformDetector.cs       # OS detection
│       ├── PlatformInfo.cs           # Platform metadata
│       └── ArchitectureDetector.cs   # CPU architecture
├── tests/
│   ├── CoordiNet-CS.Tests/           # Unit tests
│   ├── GeneratorTests/               # Template tests
│   ├── GeolocationTests/             # GeoIP tests
│   ├── PlatformTests/                # Platform tests
│   └── TunnelTests/                  # Tunnel tests
├── config/
│   └── default.json                  # Default configuration
├── assets/
│   ├── templates/
│   │   └── default.html              # Default landing page
│   └── injected/
│       ├── geolocation.js            # Client-side telemetry
│       ├── location-ui.html          # UI component
│       └── location.css              # Styling
├── Makefile                          # Native build automation
├── README.md                         # Project overview
├── SECURITY.md                       # Security policy
└── CONTRIBUTING.md                  # Contribution guidelines
```

## Development Workflow

### Building

```bash
# Development build (debug output, full symbols)
dotnet build src/CoordiNet-CS.csproj -nologo

# Release build (optimized, smaller binary)
dotnet build src/CoordiNet-CS.csproj -c Release -nologo

# Native Makefile build (recommended for distribution)
make clean
make
```

### Running

```bash
# Interactive mode (default)
dotnet run --project src/CoordiNet-CS.csproj

# Single-shot command override
dotnet run --project src/CoordiNet-CS.csproj -- --command server --port 9000

# With tunnel
dotnet run --project src/CoordiNet-CS.csproj -- --command server --tunnel ngrok
```

### Testing

```bash
# Run all tests
dotnet test tests/ -nologo

# Run specific test project
dotnet test tests/CoordiNet-CS.Tests/ -nologo

# Run with verbose output
dotnet test tests/ -nologo --logger "console;verbosity=detailed"

# Run specific test
dotnet test tests/ -nologo --filter "LocationLogicTests"
```

## Editing Templates

### Template Files

Located in `assets/templates/`:
- `default.html` - Default landing page template

### Injection Points

Templates support variable injection:
```html
<!-- Title injection -->
<h1>{{TITLE}}</h1>

<!-- Custom CSS injection -->
<style>{{CUSTOM_CSS}}</style>

<!-- Telemetry JavaScript injection -->
<script>{{TELEMETRY_JAVASCRIPT}}</script>

<!-- Redirect target injection -->
<a href="{{REDIRECT_URL}}">Click here</a>
```

### Creating Custom Templates

1. Create new file in `assets/templates/my-template.html`:
```html
<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; }
        {{CUSTOM_CSS}}
    </style>
</head>
<body>
    <h1>{{TITLE}}</h1>
    <p>{{CONTENT}}</p>
    {{TELEMETRY_JAVASCRIPT}}
</body>
</html>
```

2. Provision via CLI:
```bash
./coordinet-cs --command template --template my-template
```

3. Or via interactive mode:
```
Choose option: template
Enter template name: my-template
```

## Customizing Inline CSS

### ANSI Console Styling

The framework uses deep purple and magenta ANSI styling for telemetry display:

```csharp
// Color codes
const string ANSI_DEEP_PURPLE = "\u001b[38;5;99m";   // RGB(138, 43, 226)
const string ANSI_MAGENTA = "\u001b[38;5;201m";      // Bright magenta
const string ANSI_BOLD = "\u001b[1m";
const string ANSI_RESET = "\u001b[0m";

// Usage example
Console.WriteLine($"{ANSI_DEEP_PURPLE}═══════════════════════════════════{ANSI_RESET}");
Console.WriteLine($"{ANSI_MAGENTA}Telemetry Session {sessionId}{ANSI_RESET}");
Console.WriteLine($"{ANSI_DEEP_PURPLE}Location: {city}, {country}{ANSI_RESET}");
```

### Modifying Telemetry Display

Edit `CoreHelper.ParseAndFormatLocationAsync()` to customize telemetry rendering:

```csharp
// Location: src/Core/CoreHelper.cs

public static async Task<string> ParseAndFormatLocationAsync(TelemetryData data)
{
    var sb = new StringBuilder();
    
    // Add your custom formatting here
    sb.AppendLine($"{ANSI_DEEP_PURPLE}═══════════════════════════════════{ANSI_RESET}");
    sb.AppendLine($"{ANSI_MAGENTA}Session: {data.SessionId}{ANSI_RESET}");
    
    // Network layer
    sb.AppendLine($"{ANSI_DEEP_PURPLE}Network Layer:{ANSI_RESET}");
    sb.AppendLine($"  IP: {data.ClientIp}");
    sb.AppendLine($"  User-Agent: {data.UserAgent}");
    
    // Add more layers...
    
    return sb.ToString();
}
```

## Adding Features to CLI

### Adding a New CLI Command

1. **Create Command Module** (e.g., `src/CLI/MyNewModule.cs`):

```csharp
public class MyNewModule : IConsoleModule
{
    public string Name => "mycommand";
    public string Description => "My new command description";
    
    public async Task ExecuteAsync(ConsoleUI ui, RuntimeOptions options)
    {
        ui.WriteLine("Executing my new command...");
        await Task.Delay(1000);
        ui.WriteLine("Done!");
    }
}
```

2. **Register Module** in `CommandModuleRegistry.cs`:

```csharp
public static class CommandModuleRegistry
{
    public static IConsoleModule? GetModule(string name)
    {
        return name switch
        {
            "mycommand" => new MyNewModule(),
            // ... other commands
            _ => null
        };
    }
}
```

3. **Add CLI Argument** in `CommandLine.cs`:

```csharp
public class RuntimeOptions
{
    // ... existing options
    public string? MyNewOption { get; set; }
}

public static RuntimeOptions Parse(string[] args)
{
    var options = new RuntimeOptions();
    
    for (int i = 0; i < args.Length; i++)
    {
        switch (args[i])
        {
            case "--myoption":
                options.MyNewOption = i + 1 < args.Length ? args[++i] : null;
                break;
            // ... other cases
        }
    }
    
    return options;
}
```

4. **Test in Interactive Mode** or via command override:

```bash
./coordinet-cs --command mycommand --myoption value
```

## Extending Telemetry

### Adding New Telemetry Fields

1. **Update TelemetryData Model** (create if not exists):

```csharp
public class TelemetryData
{
    // Existing fields
    public string? ClientIp { get; set; }
    public string? UserAgent { get; set; }
    
    // New field
    /// <summary>
    /// Custom tracking parameter for new feature.
    /// Captured from client-side JavaScript payload.
    /// </summary>
    public string? MyNewField { get; set; }
}
```

2. **Add JavaScript Collector** in `assets/injected/geolocation.js`:

```javascript
// Collect new data point
const myNewField = getMyNewData();

// Include in telemetry payload
const telemetry = {
    ip: clientIp,
    userAgent: navigator.userAgent,
    myNewField: myNewField,
    // ... other fields
};

// POST to server
fetch('/log', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(telemetry)
});
```

3. **Persist to Database** in `SessionLogger.cs`:

Update SQLite schema to include new column:
```sql
ALTER TABLE sessions ADD COLUMN my_new_field TEXT;
```

Update INSERT statement:
```csharp
cmd.Parameters.AddWithValue("@myNewField", telemetry.MyNewField);
```

4. **Display in Telemetry Matrix** in `CoreHelper.cs`:

```csharp
sb.AppendLine($"  My New Field: {data.MyNewField}");
```

## Debugging

### Console Output

Enable verbose logging:
```csharp
// In Program.Main
if (options.Verbose ?? false)
{
    Console.WriteLine("[DEBUG] Verbose logging enabled");
    // Log startup details
}
```

### Breaking into Debugger

Use Visual Studio's debugger:
1. Set breakpoint in code
2. Run: `dotnet run --project src/CoordiNet-CS.csproj`
3. Debugger stops at breakpoint
4. Inspect variables, step through code

### Testing Individual Components

```bash
# Build only a specific subsystem
dotnet build src/CoordiNet-CS.csproj --projects src/CoordiNet-CS.csproj -nologo

# Run specific test
dotnet test tests/CoordiNet-CS.Tests/ --filter "TestName" -nologo
```

## Cross-Platform Considerations

### Path Handling

**Always use `System.IO.Path.Combine()`:**
```csharp
// ✅ CORRECT - Works on all platforms
string configPath = Path.Combine(
    RuntimeBootstrap.UserProfileRoot,
    ".coordinet-cs-rc",
    "config.json"
);

// ❌ WRONG - Platform-specific
string badPath = RuntimeBootstrap.UserProfileRoot + "/.coordinet-cs-rc/config.json";
```

### Platform Detection

```csharp
// Use PlatformDetector for OS-specific logic
if (PlatformDetector.IsWindows)
{
    // Windows-specific code
}
else if (PlatformDetector.IsLinux)
{
    // Linux-specific code
}
else if (PlatformDetector.IsMacOS)
{
    // macOS-specific code
}
```

## Performance Tips

1. **Use Async/Await:** Never block threads with synchronous I/O
2. **Connection Pooling:** SQLite automatically pools connections
3. **Caching:** GeoIP results are cached to minimize API calls
4. **Lazy Loading:** Load configuration only when needed
5. **Reduce Allocations:** Reuse StringBuilder, string interning

## Common Tasks

### Update Default Configuration

Edit `config/default.json`:
```json
{
  "serverPort": 8080,
  "tunnelProvider": "ngrok",
  "shortenerService": "bitly",
  "geoipProviderUrl": "https://ipapi.co/json"
}
```

### Add New Dependency

```bash
cd src
dotnet add package PackageName
```

Update `.csproj` file, then rebuild.

### Clean Build Cache

```bash
dotnet clean src/CoordiNet-CS.csproj
dotnet build src/CoordiNet-CS.csproj -nologo
```

### Publish for Distribution

```bash
dotnet publish src/CoordiNet-CS.csproj \
  -c Release \
  -p:PublishSingleFile=true \
  -p:PublishTrimmed=true
```

---

## Getting Help

- **Documentation:** Check [README.md](../README.md) and [architecture.md](../docs/architecture.md)
- **Code Examples:** Review tests in `tests/` directory
- **Issues:** Open a GitHub issue with `[QUESTION]` prefix
- **Contact:** Email voltsparx@gmail.com for questions

Happy developing! 🚀

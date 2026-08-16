# CoordiNet-CS
<p align="center">
    <img src="https://raw.githubusercontent.com/voltsparx/CoordiNet-CS/master/docs/deco/coordinet-cs-logo.png" width="300">
</p>
> Will fix this goofy ahh logo soon

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET 8.0+](https://img.shields.io/badge/.NET-8.0+-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/Language-C%23-239120?logo=csharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![Platform: Windows | Linux | macOS | Android](https://img.shields.io/badge/Platform-Windows%20%7C%20Linux%20%7C%20macOS%20%7C%20Android-0078D4)](docs/development.md)
[![Security Assessment](https://img.shields.io/badge/Type-Security%20Assessment%20Framework-red)](SECURITY.md)
[![Build Status](https://github.com/voltsparx/CoordiNet-CS/actions/workflows/dotnet.yml/badge.svg)](https://github.com/voltsparx/CoordiNet-CS/actions)
[![GitHub Stars](https://img.shields.io/github/stars/voltsparx/CoordiNet-CS?style=social)](https://github.com/voltsparx/CoordiNet-CS)

A high-performance, modular, cross-platform security assessment framework and endpoint telemetry dashboard built in C#. Engineered for precision device reconnaissance, network telemetry, and authorized penetration testing across ARM64 Linux (Android/Termux), x86/x64 Linux, Microsoft Windows, and Apple macOS architectures.

## Core Features

### Dual-Mode Execution Pipeline
- **Interactive CLI Mode**: Full feature-rich console loop with templating, mirroring, tunneling, and real-time telemetry streaming
- **Single-Shot Command Override**: Non-interactive `--command` flag execution for automation, containerization, and CI/CD integration

### Advanced IP Interception & Proxy Navigation
- **X-Forwarded-For Header Parsing**: Bypasses tunnel proxy masking by extracting true client IP from forwarding chain
- **Multi-Chain Proxy Support**: Handles CloudFlare, Ngrok, and custom proxy tunnel environments
- **Transparent Client Detection**: Distinguishes between proxy IP, forwarding chain, and true source endpoint

### Hybrid Asset Management
- **Template Provisioning Engine**: Scaffold injection payloads, geolocation capture vectors, and browser telemetry interceptors
- **Local Site Mirroring & Dependency Cloning**: Recursive mirror of target websites with automatic resource extraction (CSS, JS, images, fonts)
- **Deployment Bundle Packaging**: Archive mirrored assets with injected telemetry for seamless field deployment

### Telemetry & Tracking Framework
- **Silent Redirects**: Transparent redirect chains with automatic telemetry capture on click-through
- **Optional URL Shortening Masking**: Integrate Bit.ly, TinyURL, or custom shorteners to obfuscate tracking URLs
- **Multi-Layer Data Collection Matrix** (see table below)

### Persistent SQLite Session Storage
- **Forensic-Grade Logging**: Every endpoint contact, browser signature, geolocation attempt, and device property is stored in a durable, queryable database
- **Hidden Home-Anchored Workspace**: All runtime state, templates, and session data stored in `.coordinet-cs-rc/` within user home directory
- **Silent Background Bootstrap**: First-run initialization creates workspace structure and config without user intervention

## Telemetry Matrix

The framework captures a comprehensive multi-layer matrix of endpoint properties across four dimensional tracking:

| Layer | Category | Data Points |
|-------|----------|-------------|
| **Passive Network** | Connection Metadata | Client IP, X-Forwarded-For chain, User-Agent header, Accept-Language, Referer, Accept-Encoding |
| | HTTP Protocol | TLS version, cipher suite, HTTP method, content negotiation, cookie presence/count |
| **Coarse GeoIP Lookup** | Geographic Identity | Country code, region/state, city, postal code, timezone, ISP, ASN |
| | Location Confidence | Accuracy radius (km), IP geolocation confidence score, proxy detection flag |
| **Hardware Fingerprinting** | Device Signature | Canvas fingerprint hash, WebGL fingerprint, screen resolution, color depth, pixel ratio |
| | System Properties | Platform OS string, browser engine, font list availability, plugin list, timezone offset |
| | Browser Metadata | JavaScript enabled, WebGL capable, Geolocation API supported, LocalStorage available |
| **High-Precision GPS & Autofill** | Precise Location (Permission-Gated) | Latitude/longitude from browser Geolocation API, accuracy (meters), altitude, speed |
| | User-Provided Data | Email address via form autofill, full name, phone number (if captured via telemetry form) |

All data is indexed by session ID, timestamp, and source IP for correlation and forensic reconstruction.

## Build & Compilation

CoordiNet-CS supports multiple build methods optimized for different deployment scenarios.

### Using Makefile (Recommended for Linux/macOS/Termux)

### Prerequisites

#### Windows
```powershell
# Requires .NET SDK 8.0 or higher
dotnet --version

# Install via PowerShell (admin)
.\install\windows.ps1
```

#### Linux / macOS / Termux
```bash
# Requires .NET SDK 8.0 or higher
dotnet --version

# Install via shell
chmod +x install/linux.sh     # or install/macos.sh or install/termux.sh
./install/linux.sh
```

### Compilation Methods

#### 1. Native Makefile Build (Recommended)
```bash
# Compile using native C# compiler (csc) via .NET SDK Roslyn toolchain
make clean
make

# Output: Application-Build/coordinet-cs (Linux/macOS) or Application-Build/coordinet-cs.exe (Windows)
```

#### 2. Dotnet Build (Development)
```bash
# Build within Visual Studio / VS Code
dotnet build src/CoordiNet-CS.csproj -nologo

# Output: src/bin/Debug/net8.0/coordinet-cs
```

#### 3. Dotnet Publish (Standalone Single-File Binary)
```bash
# Create platform-specific self-contained executable
dotnet publish src/CoordiNet-CS.csproj \
  -c Release \
  -p:PublishSingleFile=true \
  -p:PublishTrimmed=true \
  -p:PublishReadyToRun=true

# Output: src/bin/Release/net8.0/publish/coordinet-cs
```

### Output Structure

After successful compilation via Makefile:

```
Application-Build/
├── coordinet-cs                          # Main executable (Linux/macOS)
├── coordinet-cs.exe                      # Main executable (Windows)
├── assets/
│   ├── templates/
│   │   └── default.html
│   └── injected/
│       ├── geolocation.js
│       ├── location-ui.html
│       └── location.css
└── config/
    └── default.json
```

## Runtime Architecture

### Hidden Workspace Structure

On first execution, the application initializes a hidden workspace in the user's home directory:

```
~/.coordinet-cs-rc/
├── config.json                           # Runtime configuration
├── coordinet.db                          # SQLite session database
├── templates/                            # Custom HTML/CSS templates
├── generated/                            # Built-in template output
├── external-websites/                    # Mirrored third-party sites
└── logs/                                 # Text session logs
```

### Execution Modes

#### Interactive Console Loop
```bash
./coordinet-cs
```
Presents a feature menu:
- `template` - Provision new injection template
- `mirror` - Clone and mirror a target website
- `bundle` - Package templates + assets for deployment
- `server` - Start local web server with telemetry collection
- `logs` - View captured session data
- `exit` - Graceful shutdown

#### Single-Shot Command Execution
```bash
./coordinet-cs --command server --port 8080 --tunnel ngrok --shorten

./coordinet-cs --command template --template default

./coordinet-cs --command mirror --website https://example.com
```

#### Direct Tunnel Provisioning
```bash
./coordinet-cs --tunnel cloudflare --port 9000
```

## CLI Command Reference

### Server Mode
```bash
./coordinet-cs --command server [--port PORT] [--tunnel PROVIDER] [--shorten]
```
- `--port PORT` - Listen on custom port (default: 8080)
- `--tunnel PROVIDER` - Tunnel via 'ngrok' or 'cloudflare' (optional)
- `--shorten` - Automatically shorten output URLs via configured shortener (requires config)

### Template Mode
```bash
./coordinet-cs --command template [--template TEMPLATE_NAME]
```
- `--template TEMPLATE_NAME` - Create template from built-in scaffold (default: 'default')

### Mirror Mode
```bash
./coordinet-cs --command mirror [--website URL]
```
- `--website URL` - Target website to recursively mirror and inject telemetry

### Bundle Mode
```bash
./coordinet-cs --command bundle
```
Package all templates and assets into a deployment bundle for field distribution.

### Global Flags
- `--help` - Display full usage information
- `--about` - Show version and authorship information
- `--nologo` - Suppress startup banner

## API & Extension Points

### WebServer Route Handlers

The local HTTP server exposes injectable routes for telemetry collection:

#### `/log` (POST)
Capture comprehensive client telemetry and persist to SQLite session database.

**Payload:**
```json
{
  "ip": "203.0.113.45",
  "userAgent": "Mozilla/5.0 ...",
  "canvas": "a1b2c3d4e5f6g7h8...",
  "screen": "1920x1080",
  "timezone": "UTC-5",
  "latitude": 40.7128,
  "longitude": -74.0060,
  "accuracy": 15
}
```

#### `/` (GET)
Serve injected landing page with embedded telemetry payload.

#### `/<path>` (GET)
Redirect to target URL with silent background telemetry capture.

### Custom Template Injection

Extend `src/Generator/TemplateInjector.cs` to create bespoke HTML/CSS/JS payloads:

```csharp
public class CustomTemplate : ITemplate
{
    public string Name => "custom-phish";
    public string HtmlContent { get; set; }
    public string CssContent { get; set; }
    public string JsPayload { get; set; }
    
    public async Task InjectTelemetryAsync(string targetUrl)
    {
        // Custom injection logic
    }
}
```

## Configuration

### config.json Reference

Located at `~/.coordinet-cs-rc/config.json`:

```json
{
  "serverPort": 8080,
  "tunnelProvider": "ngrok",
  "shortenerService": "bitly",
  "shortenerApiKey": "",
  "geoipProviderUrl": "https://ipapi.co/json",
  "enableAdvancedTelemetry": true,
  "logFormat": "json",
  "databasePath": "~/.coordinet-cs-rc/coordinet.db"
}
```

- `serverPort` - Default listening port
- `tunnelProvider` - 'ngrok' or 'cloudflare' (optional, leave empty to disable)
- `shortenerService` - URL shortener backend ('bitly', 'tinyurl', or custom)
- `shortenerApiKey` - API credentials for shortener service (optional)
- `enableAdvancedTelemetry` - Capture GPS, canvas fingerprinting, etc.
- `logFormat` - 'json' or 'csv' session output format

## Telemetry Rendering

All captured session data is rendered in a professional ANSI-styled dashboard with deep purple and magenta color blocks:

```
╔════════════════════════════════════════════════════════════════════╗
║ COORDINET SESSION TELEMETRY MATRIX                                 ║
╠════════════════════════════════════════════════════════════════════╣
║ Network Layer                                                       ║
║   IP Address:          203.0.113.45                                ║
║   Forwarded Chain:      10.0.0.1, 192.168.1.100                    ║
║   User-Agent:          Mozilla/5.0 (X11; Linux x86_64)             ║
╠════════════════════════════════════════════════════════════════════╣
║ Geographic Layer                                                    ║
║   Location:            New York, NY, USA                           ║
║   Coordinates:         40.7128°N, 74.0060°W                        ║
║   ISP:                 Example ISP (AS12345)                       ║
║   Map Link:            https://maps.google.com/?q=40.7128,-74.006 ║
╠════════════════════════════════════════════════════════════════════╣
║ Hardware Fingerprint                                                ║
║   Canvas Hash:         a1b2c3d4e5f6g7h8...                         ║
║   Screen:              1920x1080 @ 24-bit                          ║
║   Platform:            Linux x86_64                                ║
║   Browser:            Chrome 120.0                                ║
╚════════════════════════════════════════════════════════════════════╝
```

## Platform Support

| Platform | Architecture | Status | Install Script |
|----------|--------------|--------|-----------------|
| Linux | x86_64, ARM64 | ✅ Supported | `install/linux.sh` |
| macOS | x86_64, ARM64 (Apple Silicon) | ✅ Supported | `install/macos.sh` |
| Windows | x86_64 | ✅ Supported | `install/windows.ps1` |
| Android/Termux | ARM64 | ✅ Supported | `install/termux.sh` |

## Author & Attribution

**Author:** voltsparx (Niyor Kalita)  
**Email:** voltsparx@gmail.com  
**Repository:** https://github.com/voltsparx/coordinet-cs  
**License:** MIT (see [LICENSE](LICENSE))

## Legal & Compliance

**⚠️ CRITICAL NOTICE:**

This framework is designed **exclusively** for authorized defensive simulations, vulnerability assessments, and training scopes under explicit Rules of Engagement (RoE). 

Use of this toolkit against **unauthorized endpoints or systems** outside a sandboxed testing architecture is **strictly illegal** and violates computer security laws including:
- Computer Fraud and Abuse Act (CFAA) - United States
- Computer Misuse Act (CMA) - United Kingdom
- Directive 2013/40/EU - European Union

For responsible disclosure of security vulnerabilities, see [SECURITY.md](SECURITY.md).

## Support & Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines on feature requests, bug reports, and pull request submissions.

For detailed technical documentation:
- [Architecture & Data Flow](docs/architecture.md)
- [Development Guide](docs/development.md)
- [Privacy & Data Handling](docs/privacy.md)

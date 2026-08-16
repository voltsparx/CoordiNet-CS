# Architecture & Data Flow

## System Overview

`CoordiNet-CS` is a modular, asynchronous C# framework for endpoint reconnaissance and telemetry aggregation. The architecture is organized into distinct, loosely-coupled subsystems that communicate through well-defined async interfaces.

```
┌─────────────────────────────────────────────────────────────────┐
│                      CLI ENTRY POINT                            │
│                     (Program.Main)                              │
└────────────────────────┬────────────────────────────────────────┘
                         │
        ┌────────────────┼────────────────┐
        │                │                │
   ┌────▼────┐    ┌──────▼──────┐    ┌────▼──────────┐
   │ Startup │    │ Interactive │    │ Direct Command│
   │Bootstrap│    │Console Host │    │ Override      │
   └────┬────┘    └──────┬──────┘    └────┬──────────┘
        │                │                │
        └────────────────┼────────────────┘
                         │
         ┌───────────────┼─────────────────┐
         │               │                 │
    ┌────▼─────┐   ┌──────▼───────┐   ┌────▼────────┐
    │  CLI Cmd │   │  Web Server  │   │   Tunnels   │
    │ Execution│   │(HttpListener)│   │ (Ngrok/CF)  │
    └────┬─────┘   └──────┬───────┘   └────┬────────┘
         │                │                │
         └────────────────┼────────────────┘
                          │
              ┌───────────┴────────────┐
              │                        │
        ┌─────▼──────┐          ┌──────▼──────────┐
        │  Telemetry │          │  File System    │
        │  Collection│          │  & Templates    │
        └─────┬──────┘          └──────┬──────────┘
              │                        │
              │     ┌──────────────────┤
              │     │                  │
              └─────┼───────────┐      │
                    │           │      │
                ┌───▼────────┐ ┌▼──────▼─────┐
                │  SQLite DB │ │   Workspace │
                │ (Sessions) │ │ (.coordinet-│
                │            │ │  cs-rc/)    │
                └────────────┘ └─────────────┘
```

## Core Subsystems

### 1. Startup Bootstrap (Configuration.cs)

**Responsibility:** Ensure first-run initialization, hidden workspace provisioning, and runtime configuration.

**Key Functions:**
- `RuntimeBootstrap.EnsureBootstrap()` - Idempotent workspace initialization
- Creates `.coordinet-cs-rc/` in user home directory
- Generates default `config.json` on first run
- Initializes SQLite `coordinet.db` with schema
- Establishes runtime path anchors for all subsequent operations

### 2. CLI Argument Parser (CommandLine.cs)

**Responsibility:** Parse command-line arguments and construct RuntimeOptions for execution control.

### 3. Interactive Console Host (InteractiveConsoleHost.cs)

**Responsibility:** Display menu, accept user input, dispatch to command modules.

### 4. Web Server (WebServer.cs)

**Responsibility:** Local HTTP listener, route dispatch, telemetry ingestion via HttpListener.

### 5. Session Logger (SessionLogger.cs)

**Responsibility:** Durable telemetry persistence to embedded SQLite database at `~/.coordinet-cs-rc/coordinet.db`.

### 6. Template Engine (Generator/TemplateInjector.cs)

**Responsibility:** HTML/CSS/JS template scaffolding with injection points for custom payloads.

### 7. Tunnel Integration (Tunnels/TunnelManager.cs)

**Responsibility:** Expose local server to internet via tunnel providers (Ngrok, Cloudflare).

## Data Flow: Request → Persistence

### Complete Telemetry Ingestion Pipeline

1. **Client-Side Trigger**
   - Browser loads landing page or clicks redirect link
   - JavaScript collects: Canvas fingerprint, WebGL parameters, screen properties, timezone

2. **Transmission**
   - POST `/log` with JSON payload containing collected telemetry

3. **Server Reception (WebServer.cs)**
   - Extract X-Forwarded-For header chain to determine true client IP
   - Parse JSON body
   - Sanitize and validate all fields

4. **IP Enrichment (IpGeolocation.cs)**
   - Query GeoIP database/API for: Country, Region, City, ISP, Coordinates
   - Cache results to minimize API calls

5. **Database Persistence (SessionLogger.cs)**
   - Insert row into SQLite `sessions` table
   - Index by session_id, timestamp, client_ip
   - Store raw telemetry JSON for forensic analysis

6. **Display & Archival (CoreHelper.cs)**
   - Format telemetry matrix in ANSI color blocks
   - Render: Network, Geographic, Hardware, Precision layers
   - Generate clickable Google Maps link

## Concurrency & Async Safety

All I/O operations use `async/await` patterns:
- SQLite connection pooling for concurrent requests
- Non-blocking HttpListener request handlers
- Immutable configuration after initialization

## Configuration & Environment

### Runtime Configuration File

**Location:** `~/.coordinet-cs-rc/config.json`

```json
{
  "serverPort": 8080,
  "tunnelProvider": "ngrok",
  "shortenerService": "bitly",
  "geoipProviderUrl": "https://ipapi.co/json",
  "enableAdvancedTelemetry": true,
  "logFormat": "json"
}
```

### Environment Variables

```bash
export NGROK_AUTH_TOKEN="your-token-here"
export COORDINET_WORKSPACE="$HOME/custom-workspace"
export COORDINET_DB_PATH="$HOME/custom-workspace/db.sqlite"
```

## Deployment Topologies

### Topology 1: Local Testing
```
Local Machine → coordinet-cs (localhost:8080) → Browser (127.0.0.1)
```

### Topology 2: Public Tunnel
```
Local Machine → coordinet-cs (localhost:8080) → Tunnel Provider → Public Internet
```

## Logging & Observability

**Log Files:** `~/.coordinet-cs-rc/logs/`
- `coordinet-YYYY-MM-DD.log` - Application logs
- `sessions-YYYY-MM-DD.json` - Session records
- `errors.log` - Error log

**Telemetry Dashboard:**
All captured sessions rendered with deep purple ANSI blocks containing:
- Network Layer (IP, headers, User-Agent)
- Geographic Layer (location, coordinates, ISP)
- Hardware Fingerprint (canvas, WebGL, screen, platform)
- Precision Location (GPS, accuracy, altitude)

---

## Summary

The architecture emphasizes:
- **Modularity:** Each subsystem has clear responsibility
- **Async Safety:** All I/O operations are non-blocking
- **Persistence:** Durable SQLite storage for forensic analysis
- **Extensibility:** Template engine with injection points
- **Cross-Platform:** Path isolation throughout
- **Observable:** Rich logging and telemetry rendering

# Contributing to CoordiNet-CS

Thank you for your interest in contributing to the **CoordiNet-CS** security assessment framework! This document provides guidelines for participating in the project.

## Getting Started

### Fork & Clone

```bash
# Fork the repository on GitHub (using the GitHub UI)

# Clone your fork locally
git clone https://github.com/YOUR_USERNAME/CoordiNet-CS.git
cd CoordiNet-CS

# Add upstream remote for easy syncing
git remote add upstream https://github.com/voltsparx/CoordiNet-CS.git
```

### Set Up Development Environment

```bash
# Ensure .NET SDK 8.0+ is installed
dotnet --version

# Install project dependencies
dotnet restore src/CoordiNet-CS.csproj

# Verify the build works
dotnet build src/CoordiNet-CS.csproj -nologo
```

## Creating Feature Branches

Follow a clear, descriptive naming convention for your branches:

```bash
# Bug fixes
git checkout -b bugfix/description-of-issue

# Features
git checkout -b feature/description-of-feature

# Documentation improvements
git checkout -b docs/description-of-docs

# Performance improvements
git checkout -b perf/description-of-optimization

# Security hardening
git checkout -b security/description-of-hardening
```

**Examples:**
```bash
git checkout -b feature/advanced-canvas-fingerprinting
git checkout -b bugfix/x-forwarded-for-parsing
git checkout -b docs/configuration-guide
git checkout -b security/sql-injection-prevention
```

## Coding Standards

All contributions must adhere to the following standards:

### 1. Safe Asynchronous C# Patterns

All I/O operations (network, file system, database) **must** use `async/await`:

```csharp
// ✅ CORRECT
public async Task ProcessTelemetryAsync(TelemetryData data)
{
    await _sessionLogger.SaveAsync(data);
    var geoResult = await _geoService.LookupAsync(data.ClientIp);
    return FormatResult(geoResult);
}

// ❌ WRONG - Synchronous I/O blocks thread pool
public void ProcessTelemetry(TelemetryData data)
{
    _sessionLogger.Save(data);  // Blocks!
    var result = _geoService.Lookup(data.ClientIp);
}
```

### 2. Cross-Platform Path Isolation

**Always** use `System.IO.Path.Combine()`. Never hardcode path separators:

```csharp
// ✅ CORRECT - Works on Windows, Linux, macOS, Termux
string configPath = Path.Combine(
    RuntimeBootstrap.UserProfileRoot,
    ".coordinet-cs-rc",
    "config.json"
);

string templatePath = Path.Combine(
    RuntimeBootstrap.TemplateRoot,
    "templates",
    "default.html"
);

// ❌ WRONG - Breaks on Windows (uses backslash)
string badPath = RuntimeBootstrap.UserProfileRoot + "/.coordinet-cs-rc/config.json";
```

### 3. Secure String & Credential Handling

Never embed secrets, API keys, or credentials in source code:

```csharp
// ✅ CORRECT - Load from config/environment
string shortenerApiKey = config["shortenerApiKey"];
string tunnelToken = Environment.GetEnvironmentVariable("NGROK_AUTH_TOKEN");

// ❌ WRONG - Hardcoded secrets
const string API_KEY = "sk-12345abcde";
string tunnelToken = "authToken123456";
```

### 4. SQL Injection Prevention

All database queries **must** use parameterized statements:

```csharp
// ✅ CORRECT - Parameterized query
using (var cmd = _connection.CreateCommand())
{
    cmd.CommandText = "INSERT INTO sessions (ip_address, user_agent, timestamp) VALUES (@ip, @ua, @ts)";
    cmd.Parameters.AddWithValue("@ip", telemetry.ClientIp);
    cmd.Parameters.AddWithValue("@ua", telemetry.UserAgent);
    cmd.Parameters.AddWithValue("@ts", DateTime.UtcNow);
    await cmd.ExecuteNonQueryAsync();
}

// ❌ WRONG - SQL injection vulnerability
string query = $"INSERT INTO sessions VALUES ('{telemetry.ClientIp}', '{telemetry.UserAgent}')";
```

### 5. Input Validation & Sanitization

Validate and sanitize all user input:

```csharp
// ✅ CORRECT - Validate and escape
[HttpPost("/log")]
public async Task<IActionResult> LogTelemetry([FromBody] TelemetryPayload payload)
{
    // Validate input
    if (string.IsNullOrWhiteSpace(payload.ClientIp))
        return BadRequest("Invalid IP address");
    
    if (payload.UserAgent?.Length > 500)
        return BadRequest("User-Agent too long");
    
    // Sanitize for display
    var sanitized = new TelemetryData
    {
        ClientIp = payload.ClientIp.Trim(),
        UserAgent = System.Net.WebUtility.HtmlEncode(payload.UserAgent)
    };
    
    await _logger.SaveAsync(sanitized);
    return Ok();
}
```

### 6. Documentation of Tracking Parameters

Every new telemetry data point must include documentation:

```csharp
public class TelemetryData
{
    /// <summary>
    /// Client IP address, extracted from X-Forwarded-For header chain if present.
    /// Falls back to remote connection IP if header is missing.
    /// </summary>
    public string ClientIp { get; set; }
    
    /// <summary>
    /// Browser User-Agent header string.
    /// Used for device fingerprinting and browser identification.
    /// Maximum length: 500 characters (enforced).
    /// </summary>
    public string UserAgent { get; set; }
    
    /// <summary>
    /// Canvas fingerprint hash (SHA256 of canvas element rendering).
    /// Browser-side computed and submitted via telemetry payload.
    /// Used for device fingerprinting across sessions.
    /// </summary>
    public string CanvasFingerprint { get; set; }
    
    /// <summary>
    /// Browser Geolocation API result (permission-gated).
    /// Only captured if user grants explicit permission via browser prompt.
    /// Stored as latitude,longitude with accuracy radius in meters.
    /// </summary>
    public string HighPrecisionLocation { get; set; }
}
```

### 7. ANSI Console Styling

Maintain consistent deep purple and magenta ANSI styling:

```csharp
// ✅ CORRECT - Styled output
const string ANSI_DEEP_PURPLE = "\u001b[38;5;99m";  // RGB(138, 43, 226) equivalent
const string ANSI_MAGENTA = "\u001b[38;5;201m";     // Bright magenta
const string ANSI_RESET = "\u001b[0m";

Console.WriteLine($"{ANSI_DEEP_PURPLE}═══════════════════════════════════{ANSI_RESET}");
Console.WriteLine($"{ANSI_MAGENTA}Telemetry Session {sessionId}{ANSI_RESET}");
Console.WriteLine($"{ANSI_DEEP_PURPLE}Location: {city}, {country}{ANSI_RESET}");
```

## Pull Request Process

### Before Submitting

1. **Sync with upstream**:
   ```bash
   git fetch upstream
   git rebase upstream/main
   ```

2. **Test your changes**:
   ```bash
   # Run unit tests
   dotnet test tests/ -nologo
   
   # Build the project
   dotnet build src/CoordiNet-CS.csproj -nologo
   
   # Build with Makefile
   make clean && make
   ```

3. **Follow code standards** from section above

4. **Add/update documentation**:
   - Inline code comments for complex logic
   - Update README.md if behavior changes
   - Update docs/ subsystem documentation for architectural changes
   - Document new CLI flags in CONTRIBUTING.md

### Submit Pull Request

```bash
# Push your feature branch
git push origin feature/your-feature-name

# Open PR on GitHub with:
# - Clear title describing the change
# - Detailed description of what changed and why
# - Reference to any related issues (#123)
# - Testing evidence (screenshots, test output, etc.)
```

**PR Title Format:**
```
[TYPE] Short description (max 72 chars)

Types: [FEATURE], [BUGFIX], [DOCS], [PERF], [SECURITY], [REFACTOR]
```

**Example:**
```
[FEATURE] Add WebGL fingerprinting to hardware telemetry matrix

Adds WebGL parameter extraction for improved device identification:
- Queries WebGL renderer and vendor strings
- Extracts supported extensions
- Stores in SQLite sessions table
- Renders in CoreHelper telemetry dashboard

Closes #42
```

## Types of Contributions

### Features
- New telemetry data points
- Additional tunnel providers (Serveo, SSH reverse proxy, etc.)
- Template improvements
- CLI enhancements
- JavaScript client-side payload improvements

### Bug Fixes
- Runtime crashes or exceptions
- Telemetry capture failures
- Path/encoding issues on specific platforms
- SQLite connection leaks

### Documentation
- Clarifications in README, SECURITY, CONTRIBUTING
- Technical deep-dives in docs/ subsystem
- Installation guide improvements
- Example configurations

### Testing
- Unit tests for Core/ subsystem logic
- Integration tests for WebServer routes
- Platform-specific tests for install scripts

## Code Review Process

All PRs undergo review:

1. **Automated Checks**
   - Build must pass (`dotnet build`)
   - Tests must pass (`dotnet test`)
   - No compilation errors

2. **Manual Review**
   - Code standards compliance
   - Security implications
   - Performance impact
   - Documentation quality

3. **Approval & Merge**
   - Requires at least one maintainer approval
   - All feedback must be addressed
   - Approved PRs are squashed and merged to main

## Reporting Issues

Use GitHub Issues for:
- Bug reports
- Feature requests
- Documentation errors
- Questions about usage

**Issue Title Format:**
```
[BUG] or [FEATURE] or [DOCS]: Clear description
```

**Bug Report Template:**
```markdown
## Description
Clear description of the bug.

## Reproduction Steps
1. Step 1
2. Step 2
3. Step 3

## Expected Behavior
What should happen

## Actual Behavior
What actually happened

## Environment
- OS: [Windows/Linux/macOS/Termux]
- .NET Version: [output of `dotnet --version`]
- coordinet-cs Version: [commit hash or release]

## Stack Trace (if applicable)
```
[Your stack trace here]
```
```

## Community Standards

We are committed to maintaining a welcoming, inclusive community:

- **Respect** - Treat all contributors with respect regardless of background
- **Constructive** - Provide helpful, actionable feedback
- **Collaborative** - Work together toward solutions
- **Professional** - Maintain professionalism in all communication

Violations of these standards may result in contributor removal.

## Questions or Need Help?

- **Email:** voltsparx@gmail.com
- **GitHub Issues:** Ask questions in a GitHub issue with `[QUESTION]` prefix
- **Documentation:** Check [docs/](docs/) folder first

## License

By contributing to coordinet-cs, you agree that your contributions will be licensed under the MIT License.

---

**Thank you for contributing!** Your work helps make coordinet-cs a better tool for authorized security assessment and research.

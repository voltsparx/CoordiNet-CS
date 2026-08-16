# Privacy & Data Handling Policy

## Data Collection Boundary

`coordinet-cs` operates with a strict, well-defined boundary between **passive network telemetry** and **user-gated, permission-enforced precision location tracking**.

Understanding this boundary is critical for authorized operators and essential for ethical compliance.

## Passive Data Collection (No Permission Required)

The framework continuously captures passive network and system metadata that requires **no explicit user permission**:

### Network Layer
These data points are **always collected** from HTTP requests:
- **Client IP Address** (from X-Forwarded-For header chain or direct connection)
- **User-Agent String** (browser, OS, version)
- **Accept-Language Header** (language preferences)
- **Referer Header** (click-through source)
- **HTTP Method** (GET, POST, etc.)
- **TLS/SSL Version** (connection security level)
- **Cipher Suite** (encryption algorithm in use)
- **Connection Timestamp** (UTC date/time)

**No user action required.** These are standard HTTP metadata captured by any web server.

### System Fingerprinting Layer
Via client-side JavaScript (executed without special permission):
- **Canvas Fingerprint** (SHA256 hash of canvas rendering)
- **WebGL Parameters** (GPU vendor, renderer, supported extensions)
- **Screen Resolution** (pixel dimensions, color depth)
- **Pixel Ratio** (device scaling factor)
- **Timezone Offset** (UTC offset in milliseconds)
- **Platform String** (OS identification)
- **Browser Engine** (Chromium, Gecko, WebKit)
- **Plugin/Extension List** (if available to JavaScript)

**User Action Required:** User loads web page. Script executes automatically. No additional consent needed beyond page load.

### Geographic Inference Layer (GeoIP Lookup)
From the client IP address:
- **Country Code** (ISO 3166-1 alpha-2, e.g., "US")
- **Region/State Name** (e.g., "New York")
- **City Name** (e.g., "New York City")
- **Postal Code**
- **Timezone** (e.g., "America/New_York")
- **ISP Organization** (Internet Service Provider)
- **Autonomous System Number (ASN)** (BGP routing identifier)
- **Accuracy Radius** (km: typically 5-50 km depending on provider)
- **Latitude / Longitude** (approximate center of accuracy circle)

**Limitation:** GeoIP is **coarse-grained.** Typical accuracy is 5-50 km. It represents the approximate location of the ISP/data center, not the device.

**No user permission required.** GeoIP is a passive inference from publicly routable IP addresses.

---

## High-Precision Location Tracking (Permission-Enforced)

The framework supports capture of **precise GPS coordinates**, but only through the browser's **Geolocation API**, which is explicitly permission-gated.

### Browser Geolocation API (Browser Permission Required)

When enabled, the framework **requests explicit browser permission**:

```javascript
if (navigator.geolocation) {
    navigator.geolocation.getCurrentPosition(
        (position) => {
            // User ALLOWED permission
            const latitude = position.coords.latitude;
            const longitude = position.coords.longitude;
            const accuracy = position.coords.accuracy; // meters
            
            // Send to server
            fetch('/log', {
                method: 'POST',
                body: JSON.stringify({ latitude, longitude, accuracy })
            });
        },
        (error) => {
            // User DENIED permission or unavailable
            console.log("Permission denied or unavailable");
        }
    );
}
```

**User Interaction:** Browser displays a permission prompt:
```
Location
Allow [website] to access your location?
[Allow] [Block]
```

**Data Captured (if user allows):**
- **Latitude / Longitude** (precise WGS84 coordinates, ~10m accuracy)
- **Accuracy** (estimated error radius in meters)
- **Altitude** (elevation above sea level, if available)
- **Altitude Accuracy** (estimated error, if available)
- **Heading** (compass bearing, if device supports)
- **Speed** (velocity in meters/second, if available)

**Critical:** This data is **only captured if the user explicitly clicks "Allow"** in the browser permission prompt.

### Autofill-Based Data Capture

If the framework includes HTML forms with autofill hints:
- **Email Address** (if user's browser autofill provides it)
- **Full Name** (if user's browser autofill provides it)
- **Phone Number** (if user's browser autofill provides it)

**Permission Boundary:** This is **not** HTML5 Geolocation API permission. It relies on browser autofill behavior, which is controlled by user's browser settings and saved credentials.

**User Responsibility:** User must enable autofill in their browser settings and save credentials for this to populate.

---

## Data Storage & Persistence

### Local SQLite Database

All collected data is stored in an embedded SQLite database:

**Location:** `~/.coordinet-cs-rc/coordinet.db`

**Schema:**
```sql
CREATE TABLE sessions (
    id TEXT PRIMARY KEY,
    session_id TEXT NOT NULL,
    timestamp TEXT NOT NULL,
    client_ip TEXT NOT NULL,
    user_agent TEXT,
    canvas_fingerprint TEXT,
    screen_resolution TEXT,
    timezone TEXT,
    country TEXT,
    region TEXT,
    city TEXT,
    isp TEXT,
    asn TEXT,
    latitude REAL,              -- HIGH-PRECISION (GeoIP is coarse)
    longitude REAL,             -- HIGH-PRECISION (GeoIP is coarse)
    accuracy_meters INTEGER,    -- Accuracy from browser Geolocation API
    is_proxy BOOLEAN,
    telemetry_data TEXT,        -- JSON blob of raw data
    created_at TEXT DEFAULT CURRENT_TIMESTAMP
);
```

**Encryption:** SQLite database is **not encrypted by default**. Operators are responsible for encrypting the database at rest if handling sensitive data.

**Access:** Database is stored in user home directory and readable by the application user only (standard file permissions).

### Data Retention

**Default Retention:** All data is retained indefinitely in the SQLite database until manually deleted.

**Manual Cleanup:**
```bash
# Delete all sessions older than 30 days
sqlite3 ~/.coordinet-cs-rc/coordinet.db "DELETE FROM sessions WHERE created_at < datetime('now', '-30 days');"
```

---

## User Privacy Recommendations

### For Operators (Ethical Usage)

1. **Informed Consent:** Users should be informed that:
   - Network metadata will be captured passively
   - GeoIP lookup will be performed (coarse geolocation)
   - Permission will be requested for precise GPS
   - Data will be stored in a database

2. **Clear Privacy Notice:** Include a visible privacy notice on landing page:
   ```html
   <div class="privacy-notice">
       <h2>Privacy Notice</h2>
       <p>This page collects:</p>
       <ul>
           <li>Your IP address and browser information</li>
           <li>Your approximate location via IP geolocation</li>
           <li>Your device screen and browser details</li>
           <li>Precise GPS location (if you permit)</li>
       </ul>
       <p>This data is used for [authorized purpose].</p>
   </div>
   ```

3. **Permission Enforcement:** Only request Geolocation API permission when necessary.

4. **Data Minimization:** Only collect the fields you actually need.

5. **Secure Storage:** Encrypt the database if handling sensitive data:
   ```bash
   # Use encrypted storage via filesystem encryption or database-level encryption
   ```

6. **Retention Limits:** Regularly delete old session data:
   ```bash
   # Automated cleanup (monthly)
   0 0 1 * * sqlite3 ~/.coordinet-cs-rc/coordinet.db "DELETE FROM sessions WHERE created_at < datetime('now', '-90 days');"
   ```

### For End Users (Protection)

1. **Review Permission Prompts:** Carefully read permission requests before allowing.
2. **Disable Autofill:** If concerned about autofill data capture, disable browser autofill.
3. **Use VPN:** Use a VPN to mask your real IP address from GeoIP lookup.
4. **Block JavaScript:** Some browser extensions allow per-site JavaScript blocking.
5. **Privacy Mode:** Use browser private/incognito mode to avoid persistent cookies.

---

## Legal & Regulatory Compliance

### GDPR (EU)

**Personal Data:** GeoIP location + device fingerprint + IP address may constitute "personal data" under GDPR.

**Requirements:**
- Lawful basis for processing (consent, legitimate interest, etc.)
- Privacy notice in clear language
- Data subject rights (access, deletion, portability)
- Data Processing Agreement (DPA) if using third-party processors

### CCPA (California)

**Consumer Rights:**
- Right to know what data is collected
- Right to delete personal information
- Right to opt-out of sale or sharing
- Right to non-discrimination

### PIPEDA (Canada)

**Requirements:**
- Obtain consent before collecting personal information
- Provide privacy notice
- Secure handling and storage
- Data subject rights

### Other Jurisdictions

Verify compliance with applicable laws in your jurisdiction before operational use.

---

## Third-Party Data Sharing

### GeoIP API

If using an external GeoIP provider (e.g., ipapi.co, MaxMind):
- **Data Shared:** Client IP address
- **Purpose:** Geographic lookup
- **Provider Privacy:** Review provider's privacy policy
- **Example (ipapi.co):** https://ipapi.co/privacy/

### URL Shortener Service

If using an optional URL shortener (Bit.ly, TinyURL):
- **Data Shared:** Full URL with embedded telemetry tracking
- **Purpose:** URL shortening/obfuscation
- **Provider Privacy:** Review shortener's privacy policy
- **Recommendation:** Use self-hosted shortener if possible

### Tunnel Provider

If using Ngrok or Cloudflare Tunnel:
- **Data Transmitted:** All HTTP request/response data through tunnel
- **Note:** Tunnel provider can theoretically inspect traffic
- **Recommendation:** Use HTTPS for end-to-end encryption

**Privacy Best Practice:** Minimize data sharing with third parties. Only use external services when necessary.

---

## Data Subject Rights

### Right to Access

Users have the right to request their data:
```bash
# Export user's session data
sqlite3 ~/.coordinet-cs-rc/coordinet.db "SELECT * FROM sessions WHERE client_ip = '203.0.113.45';" > user_data.json
```

### Right to Deletion

Users have the right to request deletion:
```bash
# Delete all sessions for a user
sqlite3 ~/.coordinet-cs-rc/coordinet.db "DELETE FROM sessions WHERE client_ip = '203.0.113.45';"
```

### Right to Portability

Operators should export data in standard formats (CSV, JSON):
```bash
# Export all session data as JSON
sqlite3 ~/.coordinet-cs-rc/coordinet.db ".mode json" "SELECT * FROM sessions;" > sessions.json
```

---

## Security Best Practices

1. **Database Encryption:** Encrypt `coordinet.db` at rest
   ```bash
   # Linux: Use LUKS encryption
   # Windows: Use BitLocker or EFS
   # macOS: Use FileVault
   ```

2. **Network Encryption:** Always use HTTPS for web endpoints
   ```csharp
   // WebServer.cs should bind to HTTPS if handling sensitive data
   ```

3. **Access Control:** Restrict database file permissions
   ```bash
   chmod 600 ~/.coordinet-cs-rc/coordinet.db
   ```

4. **Audit Logging:** Log all database access
   ```csharp
   // Log all telemetry writes
   Logger.Info($"Saved session {sessionId} from {clientIp}");
   ```

5. **Credential Management:** Never store API keys in source code
   ```bash
   export NGROK_AUTH_TOKEN="your-token"
   export SHORTENER_API_KEY="your-key"
   ```

---

## Summary

**Passive Collection (Always):**
- Network metadata (IP, User-Agent, headers)
- System fingerprinting (canvas, WebGL, screen, browser)
- Coarse GeoIP location (~5-50km accuracy)

**Permission-Gated Collection (User Control):**
- Precise GPS via browser Geolocation API (user must click "Allow")
- Autofill data (user browser settings control)

**Storage:**
- All data in SQLite `~/.coordinet-cs-rc/coordinet.db`
- Encrypted storage recommended
- Users can request access/deletion per GDPR/CCPA/PIPEDA

**Responsibility:**
- Operators: Ensure informed consent, comply with regulations
- Users: Review permissions, use privacy tools

---

## Questions or Concerns?

- **Privacy Issue:** Email voltsparx@gmail.com with details
- **Data Subject Request:** File formal request via GitHub Issues with `[DATA-REQUEST]` prefix
- **Regulatory Compliance:** Review [SECURITY.md](SECURITY.md) for authorized use policy

**Last Updated:** 2026-08-17  
**Version:** 1.0

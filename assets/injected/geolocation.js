(function() {
  'use strict';

  // Canvas fingerprinting for device signature
  function getCanvasFingerprint() {
    try {
      const canvas = document.createElement('canvas');
      const ctx = canvas.getContext('2d');
      ctx.textBaseline = 'top';
      ctx.font = '14px Arial';
      ctx.textBaseline = 'alphabetic';
      ctx.fillStyle = '#f60';
      ctx.fillRect(125, 1, 62, 20);
      ctx.fillStyle = '#069';
      ctx.fillText('CoordiNet-CS', 2, 15);
      ctx.fillStyle = 'rgba(102, 204, 0, 0.7)';
      ctx.fillText('CoordiNet-CS', 4, 17);
      
      return canvas.toDataURL();
    } catch (e) {
      return 'unsupported';
    }
  }

  // WebGL fingerprinting
  function getWebGLFingerprint() {
    try {
      const canvas = document.createElement('canvas');
      const gl = canvas.getContext('webgl') || canvas.getContext('experimental-webgl');
      if (!gl) return 'unavailable';
      
      const debugInfo = gl.getExtension('WEBGL_debug_renderer_info');
      return debugInfo ? {
        renderer: gl.getParameter(debugInfo.UNMASKED_RENDERER_WEBGL),
        vendor: gl.getParameter(debugInfo.UNMASKED_VENDOR_WEBGL)
      } : 'limited';
    } catch (e) {
      return 'error';
    }
  }

  // Collect device telemetry
  function collectTelemetry(callback) {
    const telemetry = {
      timestamp: new Date().toISOString(),
      userAgent: navigator.userAgent,
      language: navigator.language,
      platform: navigator.platform,
      hardwareConcurrency: navigator.hardwareConcurrency || 'unknown',
      deviceMemory: navigator.deviceMemory || 'unknown',
      screenWidth: window.screen.width,
      screenHeight: window.screen.height,
      screenColorDepth: window.screen.colorDepth,
      screenPixelDepth: window.screen.pixelDepth,
      timezoneOffset: new Date().getTimezoneOffset(),
      timezone: Intl.DateTimeFormat().resolvedOptions().timeZone,
      canvasFingerprint: getCanvasFingerprint(),
      webglFingerprint: getWebGLFingerprint(),
      localStorageEnabled: (function() { try { const t = '__test__'; localStorage.setItem(t, t); localStorage.removeItem(t); return true; } catch (e) { return false; } })(),
      sessionStorageEnabled: (function() { try { const t = '__test__'; sessionStorage.setItem(t, t); sessionStorage.removeItem(t); return true; } catch (e) { return false; } })(),
      cookiesEnabled: navigator.cookieEnabled,
      doNotTrack: navigator.doNotTrack,
      plugins: Array.from(navigator.plugins || []).map(p => ({ name: p.name, description: p.description }))
    };

    // Request high-precision geolocation if available
    if (navigator.geolocation) {
      navigator.geolocation.getCurrentPosition(
        function(position) {
          telemetry.latitude = position.coords.latitude;
          telemetry.longitude = position.coords.longitude;
          telemetry.accuracy = position.coords.accuracy;
          telemetry.altitude = position.coords.altitude;
          telemetry.altitudeAccuracy = position.coords.altitudeAccuracy;
          telemetry.heading = position.coords.heading;
          telemetry.speed = position.coords.speed;
          telemetry.timestamp = position.timestamp;
          callback(telemetry);
        },
        function(error) {
          telemetry.geolocationError = {
            code: error.code,
            message: error.message
          };
          callback(telemetry);
        },
        {
          enableHighAccuracy: true,
          timeout: 10000,
          maximumAge: 0
        }
      );
    } else {
      callback(telemetry);
    }
  }

  // Send telemetry to server
  function sendTelemetry(telemetry) {
    const payload = JSON.stringify(telemetry);
    
    if (navigator.sendBeacon) {
      navigator.sendBeacon('/log', payload);
    } else {
      fetch('/log', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: payload,
        keepalive: true
      }).catch(function(error) {
        console.debug('Telemetry transmission failed:', error);
      });
    }
  }

  // Initialize on document ready
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', function() {
      collectTelemetry(sendTelemetry);
    });
  } else {
    collectTelemetry(sendTelemetry);
  }

  // Also send on page unload
  window.addEventListener('beforeunload', function() {
    collectTelemetry(sendTelemetry);
  });
})();

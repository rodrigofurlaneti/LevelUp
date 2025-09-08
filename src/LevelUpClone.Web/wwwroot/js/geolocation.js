// src/js/geolocation.js
// ============================================================================
// Geolocalização + Telemetria de Ambiente + CorrelationId (frontend)
// - Um único POST por page load (com retry opcional)
// - Cache de coordenadas por 24h (localStorage)
// - SessionId por aba (sessionStorage)
// - Header "X-Correlation-Id" + envio no body (PascalCase => Dapper/SQL)
// ============================================================================

(function () {
    // Evita duplicação do script
    if (window.__FSI_GEO_LOADED__) return;
    window.__FSI_GEO_LOADED__ = true;

    // ==========================
    // Config
    // ==========================
    const DEBUG = true;
    const CACHE_KEY = "fsi.clientGeo";
    const CACHE_TTL_MS = 24 * 60 * 60 * 1000; // 24h
    const TIMEOUT_MS = 8000;
    const POST_RETRIES = 2;          // tentativas extras além da primeira
    const RETRY_BASE_MS = 500;       // backoff exponencial
    const PREFLIGHT_DIAGNOSTIC = false; // para depurar CORS

    // API base (padrão 7121). Pode sobrescrever com window.__FSI_API_BASE.
    const DEFAULT_API_PORT = "7121";
    const API_BASE =
        (typeof window.__FSI_API_BASE === "string" && window.__FSI_API_BASE.trim())
            ? window.__FSI_API_BASE.trim()
            : `${location.protocol}//${location.hostname}:${DEFAULT_API_PORT}`;

    // Endpoints
    const GEO_ENDPOINT = `${API_BASE}/api/geolog`;
    // opcional de health: const DB_ENDPOINT = `${API_BASE}/api/health/database`;

    // Flags de ciclo
    let started = false;

    // ==========================
    // Log helpers
    // ==========================
    const now = () => Date.now();
    function dlog(...a) { if (DEBUG) console.log("[Geo]", ...a); }
    function derr(...a) { if (DEBUG) console.warn("[Geo:warn]", ...a); }

    // ==========================
    // CorrelationId & SessionId
    // ==========================
    function generateCorrelationId() {
        return (crypto.randomUUID && crypto.randomUUID())
            || `cid-${Date.now()}-${Math.random().toString(16).slice(2)}`;
    }
    const correlationId = generateCorrelationId();

    function getOrCreateSessionId() {
        let sid = sessionStorage.getItem("sid");
        if (!sid) {
            sid = (crypto.randomUUID && crypto.randomUUID())
                || `sid-${Date.now()}-${Math.random().toString(16).slice(2)}`;
            sessionStorage.setItem("sid", sid);
        }
        return sid;
    }

    // ==========================
    // Bot / Device / UA parsing
    // ==========================
    function detectBot(uaRaw) {
        const ua = (uaRaw || "").toLowerCase();
        const patterns = [
            "googlebot", "bingbot", "yandexbot", "duckduckbot", "baiduspider", "applebot",
            "facebot", "facebookexternalhit", "twitterbot", "slackbot", "discordbot",
            "linkedinbot", "semrushbot", "ahrefsbot", "mj12bot", "petalbot", "sogou",
            "exabot", "ia_archiver", "adsbot-google", "apis-google", "mediapartners-google"
        ];
        const match = patterns.find(p => ua.includes(p));
        const generic = /\b(bot|crawler|spider|preview)\b/.test(ua);
        return { isBot: Boolean(match || generic), botName: match || (generic ? "GenericBot" : "") };
    }

    function detectDevice(uaRaw, hints) {
        const ua = (uaRaw || "").toLowerCase();
        const chMobile = typeof hints?.mobile === "boolean" ? hints.mobile : null;
        const chModel = hints?.model || "";

        const isIPad = /ipad/.test(ua) || (/macintosh/.test(ua) && "ontouchstart" in window);
        const isIPhone = /iphone/.test(ua);
        const isAndroid = /android/.test(ua);
        const isAndroidTablet = isAndroid && !/mobile/.test(ua);
        const isTabletKeyword = /(tablet|tab)/.test(ua);
        const isMobileKeyword = /(mobile|phone)/.test(ua);

        let deviceType = "desktop";
        if (chMobile === true || isIPhone || (isAndroid && !isAndroidTablet) || isMobileKeyword) {
            deviceType = "mobile";
        } else if (isIPad || isAndroidTablet || isTabletKeyword) {
            deviceType = "tablet";
        }

        let deviceModel = chModel || "";
        if (!deviceModel) {
            if (isIPhone) deviceModel = "iPhone";
            else if (isIPad) deviceModel = "iPad";
            else if (isAndroid) {
                const m = ua.match(/(sm-[\w-]+|moto[\w-]+|pixel [\w-]+|mi [\w-]+|redmi [\w-]+|oneplus [\w-]+)/i);
                if (m) deviceModel = m[0];
            }
        }
        const touchPoints = navigator.maxTouchPoints || 0;
        return { deviceType, deviceModel, touchPoints };
    }

    function parseUA(uaRaw) {
        const ua = (uaRaw || "").trim();
        const rx = (r) => r.exec(ua);
        const ver = (m, i = 1) => (m && m[i]) ? m[i].replace(/_/g, ".") : "";
        const has = (s) => ua.toLowerCase().includes(s.toLowerCase());
        let m;

        let browser = "Unknown", browserVersion = "";
        if (m = rx(/SamsungBrowser\/([\d.]+)/)) { browser = "Samsung Internet"; browserVersion = ver(m); }
        else if (m = rx(/Edg\/([\d.]+)/)) { browser = "Microsoft Edge"; browserVersion = ver(m); }
        else if (m = rx(/OPR\/([\d.]+)/)) { browser = "Opera"; browserVersion = ver(m); }
        else if (!has("crios") && (m = rx(/Chrome\/([\d.]+)/))) { browser = "Chrome"; browserVersion = ver(m); }
        else if (m = rx(/CriOS\/([\d.]+)/)) { browser = "Chrome (iOS)"; browserVersion = ver(m); }
        else if (m = rx(/Firefox\/([\d.]+)/)) { browser = "Firefox"; browserVersion = ver(m); }
        else if (m = rx(/FxiOS\/([\d.]+)/)) { browser = "Firefox (iOS)"; browserVersion = ver(m); }
        else if (has("safari") && !has("chrome") && (m = rx(/Version\/([\d.]+)/))) { browser = "Safari"; browserVersion = ver(m); }
        else if (has("; wv") || has(" wv)")) { browser = "Android WebView"; m = rx(/Version\/([\d.]+)/); browserVersion = ver(m) || ""; }
        else if (m = rx(/AppleWebKit\/([\d.]+)/)) { browser = "WebKit-based"; browserVersion = ver(m); }

        let operatingSystem = "Unknown", osVersion = "";
        if (m = rx(/Windows NT ([\d.]+)/)) {
            operatingSystem = "Windows";
            const map = { "10.0": "10/11", "6.3": "8.1", "6.2": "8", "6.1": "7", "6.0": "Vista", "5.1": "XP" };
            const nt = ver(m);
            osVersion = map[nt] ? map[nt] : nt;
        } else if (m = rx(/iPhone OS ([\d_]+)/)) { operatingSystem = "iOS"; osVersion = ver(m); }
        else if (m = rx(/CPU OS ([\d_]+)/)) { operatingSystem = "iOS"; osVersion = ver(m); }
        else if (m = rx(/iPad; CPU ([\w ]+) OS ([\d_]+)/)) { operatingSystem = "iPadOS"; osVersion = ver(m, 2); }
        else if (m = rx(/Mac OS X ([\d_]+)/)) { operatingSystem = "macOS"; osVersion = ver(m); }
        else if (m = rx(/Android ([\d.]+)/)) { operatingSystem = "Android"; osVersion = ver(m); }
        else if (/Linux/i.test(ua)) { operatingSystem = "Linux"; }

        let architecture = "";
        if (/(WOW64|Win64|x64|amd64)/i.test(ua)) architecture = "x64";
        else if (/(arm64|aarch64)/i.test(ua)) architecture = "arm64";
        else if (/(i686|x86)/i.test(ua)) architecture = "x86";

        return { browser, browserVersion, operatingSystem, osVersion, architecture };
    }

    // ==========================
    // Client Hints + enrich
    // ==========================
    async function getEnvInfoAsync() {
        const tz = (() => {
            try { return Intl.DateTimeFormat().resolvedOptions().timeZone || null; }
            catch { return null; }
        })();

        const connRaw = navigator.connection || navigator.mozConnection || navigator.webkitConnection;
        const connection = connRaw ? {
            effectiveType: connRaw.effectiveType ?? null,
            downlink: connRaw.downlink ?? null,
            rtt: connRaw.rtt ?? null,
            saveData: connRaw.saveData ?? null
        } : null;

        const ua = navigator.userAgent || null;
        let parsed = parseUA(ua);

        let ch = { brands: [], mobile: null, model: "", uaFullVersion: "", platform: "", platformVersion: "", architecture: "", bitness: "" };
        try {
            if (navigator.userAgentData) {
                ch.brands = navigator.userAgentData.brands || [];
                ch.mobile = navigator.userAgentData.mobile ?? null;

                if (navigator.userAgentData.getHighEntropyValues) {
                    const hi = await navigator.userAgentData.getHighEntropyValues([
                        "architecture", "bitness", "platform", "platformVersion", "model", "uaFullVersion"
                    ]);
                    Object.assign(ch, hi);
                }

                if (ch.brands?.length) {
                    const brand = ch.brands.find(b => !/Not.*Brand/i.test(b.brand)) || ch.brands[0];
                    if (brand?.brand) {
                        parsed.browser = parsed.browser === "Unknown" ? brand.brand : parsed.browser;
                        parsed.browserVersion = ch.uaFullVersion || brand.version || parsed.browserVersion;
                    }
                }
                if (ch.platform) parsed.operatingSystem = parsed.operatingSystem === "Unknown" ? ch.platform : parsed.operatingSystem;
                if (ch.platformVersion) parsed.osVersion = parsed.osVersion || ch.platformVersion;
                if (ch.architecture) parsed.architecture = parsed.architecture || ch.architecture;
                else if (ch.bitness === "64") parsed.architecture = parsed.architecture || "x64";
            }
        } catch { /* ignore */ }

        const bot = detectBot(ua);
        const device = detectDevice(ua, { mobile: ch.mobile, model: ch.model });

        return {
            ua,
            browser: parsed.browser || "",
            browserVersion: parsed.browserVersion || "",
            operatingSystem: parsed.operatingSystem || "",
            osVersion: parsed.osVersion || "",
            architecture: parsed.architecture || "",
            deviceType: device.deviceType,
            deviceModel: device.deviceModel,
            touchPoints: device.touchPoints,
            isBot: bot.isBot,
            botName: bot.botName,
            language: navigator.language || null,
            languages: navigator.languages || null,
            platform: navigator.platform || null,
            online: typeof navigator.onLine === "boolean" ? navigator.onLine : null,
            timeZone: tz,
            screenWidth: window.screen?.width ?? null,
            screenHeight: window.screen?.height ?? null,
            dpr: window.devicePixelRatio || 1,
            referrer: document.referrer || null,
            page: location?.href || null,
            connection
        };
    }

    // ==========================
    // Cache geolocalização
    // ==========================
    function readCache() {
        try {
            const raw = localStorage.getItem(CACHE_KEY);
            if (!raw) return null;
            const data = JSON.parse(raw);
            if (!data || !data.ts || (now() - data.ts) > CACHE_TTL_MS) return null;
            dlog("cache hit", data);
            return data;
        } catch (e) { derr("readCache error", e); return null; }
    }

    function writeCache(coords, place) {
        try {
            const payload = { ts: now(), coords, place: place || null };
            localStorage.setItem(CACHE_KEY, JSON.stringify(payload));
            dlog("cache write", payload);
        } catch (e) { derr("writeCache error", e); }
    }

    // ==========================
    // Geolocalização
    // ==========================
    function getPosition(options) {
        return new Promise((resolve, reject) => {
            if (!navigator.geolocation?.getCurrentPosition)
                return reject(new Error("Geolocation not supported"));

            let settled = false;
            const opt = Object.assign({ enableHighAccuracy: true, maximumAge: 600000, timeout: 10000 }, options || {});
            dlog("calling geolocation with", opt);

            const timer = setTimeout(() => {
                if (!settled) { settled = true; reject(new Error("Geolocation timeout")); }
            }, opt.timeout);

            navigator.geolocation.getCurrentPosition(
                pos => { if (!settled) { settled = true; clearTimeout(timer); resolve(pos); } },
                err => { if (!settled) { settled = true; clearTimeout(timer); reject(err); } },
                opt
            );
        });
    }

    function pickAllFieldsFrom(pos, place) {
        const c = pos.coords;
        return {
            lat: c.latitude,
            lon: c.longitude,
            accuracy: c.accuracy ?? null,
            altitude: c.altitude ?? null,
            altitudeAccuracy: c.altitudeAccuracy ?? null,
            speed: c.speed ?? null,
            heading: c.heading ?? null,
            ts: pos.timestamp || Date.now(),
            city: place || null
        };
    }

    // ==========================
    // DOM helpers (opcionais)
    // ==========================
    function setIndicator(elId, ok, titleOk, titleErr) {
        const el = document.getElementById(elId);
        if (!el) return;
        el.style.backgroundColor = ok ? "green" : "red";
        el.title = ok ? titleOk : titleErr;
    }

    function setDom(fields) {
        // Preenche elementos com [data-geo] se existirem na página
        const flat = {
            lat: fields.geo?.lat ?? null,
            lon: fields.geo?.lon ?? null,
            accuracy: fields.geo?.accuracy ?? null,
            altitude: fields.geo?.altitude ?? null,
            altitudeAccuracy: fields.geo?.altitudeAccuracy ?? null,
            speed: fields.geo?.speed ?? null,
            heading: fields.geo?.heading ?? null,
            ts: fields.geo?.ts ?? null,
            city: fields.geo?.city ?? "",
            ua: fields.env?.ua ?? "",
            language: fields.env?.language ?? "",
            timeZone: fields.env?.timeZone ?? "",
            screenWidth: fields.env?.screenWidth ?? null,
            screenHeight: fields.env?.screenHeight ?? null,
            dpr: fields.env?.dpr ?? null,
            online: fields.env?.online ?? null,
            connectionType: fields.env?.connection?.effectiveType ?? "",
            connectionDownlink: fields.env?.connection?.downlink ?? null,
            connectionRtt: fields.env?.connection?.rtt ?? null,
            saveData: fields.env?.connection?.saveData ?? null,
            platform: fields.env?.platform ?? "",
            page: fields.env?.page ?? "",
            referrer: fields.env?.referrer ?? "",
            browser: fields.env?.browser ?? "",
            browserVersion: fields.env?.browserVersion ?? "",
            operatingSystem: fields.env?.operatingSystem ?? "",
            osVersion: fields.env?.osVersion ?? "",
            architecture: fields.env?.architecture ?? "",
            deviceType: fields.env?.deviceType ?? "",
            deviceModel: fields.env?.deviceModel ?? "",
            touchPoints: fields.env?.touchPoints ?? 0,
            isBot: fields.env?.isBot ?? false,
            botName: fields.env?.botName ?? ""
        };

        const map = new Map(Object.entries(flat));
        document.querySelectorAll("[data-geo]").forEach(el => {
            const key = el.getAttribute("data-geo");
            if (!key) return;
            if (key === "lat" && typeof flat.lat === "number") el.textContent = flat.lat.toFixed(6);
            else if (key === "lon" && typeof flat.lon === "number") el.textContent = flat.lon.toFixed(6);
            else if (key === "accuracy" && flat.accuracy != null) el.textContent = Math.round(flat.accuracy).toString();
            else if (key === "altitude" && flat.altitude != null) el.textContent = flat.altitude.toFixed(1);
            else if (key === "altitudeAccuracy" && flat.altitudeAccuracy != null) el.textContent = Math.round(flat.altitudeAccuracy).toString();
            else if (key === "speed" && flat.speed != null) el.textContent = flat.speed.toFixed(2);
            else if (key === "heading" && flat.heading != null) el.textContent = flat.heading.toFixed(0);
            else if (key === "ts" && flat.ts != null) el.textContent = new Date(flat.ts).toLocaleString();
            else if (map.has(key)) el.textContent = String(map.get(key) ?? "");
        });

        window.__fsiGeo = fields;
        dlog("DOM set", fields);
        window.dispatchEvent(new CustomEvent("fsi:geo", { detail: fields }));
    }

    // ==========================
    // Fetch helpers (CORS/timeout/retry)
    // ==========================
    function isLikelyNetworkError(err) {
        const msg = String(err && err.message || err || "").toLowerCase();
        return (
            msg.includes("networkerror") ||
            msg.includes("failed to fetch") ||
            msg.includes("timeout") ||
            msg.includes("abort") ||
            msg.includes("net::") ||
            msg.includes("connection") ||
            msg.includes("ssl") ||
            msg.includes("tls")
        );
    }

    async function postWithRetry(url, body, tries = POST_RETRIES) {
        let attempt = 0, lastErr;
        while (attempt <= tries) {
            try {
                const res = await fetch(url, {
                    method: "POST",
                    mode: "cors",
                    credentials: "omit", // importante quando AllowAnyOrigin no CORS
                    cache: "no-store",
                    headers: {
                        "Content-Type": "application/json",
                        "X-Correlation-Id": correlationId
                    },
                    body: JSON.stringify(body),
                    referrerPolicy: "strict-origin-when-cross-origin"
                });
                if (!res.ok) throw new Error(`HTTP ${res.status}`);
                return res;
            } catch (err) {
                lastErr = err;
                if (!isLikelyNetworkError(err) || attempt === tries) break;
                const wait = RETRY_BASE_MS * Math.pow(2, attempt);
                dlog(`retry in ${wait}ms (attempt ${attempt + 1}/${tries + 1})`, err);
                await new Promise(r => setTimeout(r, wait));
            }
            attempt++;
        }
        throw lastErr;
    }

    async function preflightCheck(url) {
        try {
            const res = await fetch(url, {
                method: "OPTIONS",
                mode: "cors",
                credentials: "omit",
                headers: {
                    "Access-Control-Request-Method": "POST",
                    "Access-Control-Request-Headers": "Content-Type, X-Correlation-Id",
                    "Origin": location.origin
                }
            });
            dlog("preflight", res.status, [...res.headers.entries()]);
            return res.ok || res.status === 204;
        } catch (e) {
            derr("preflight error", e);
            return false;
        }
    }

    // ==========================
    // Payload (PascalCase) — De-Para comentado no topo
    // ==========================
    function toPascalPayload(geo, env, error) {
        return {
            Latitude: geo?.lat ?? null,
            Longitude: geo?.lon ?? null,
            AccuracyMeters: geo?.accuracy ?? null,
            AltitudeMeters: geo?.altitude ?? null,
            AltitudeAccuracyMeters: geo?.altitudeAccuracy ?? null,
            SpeedMetersPerSecond: geo?.speed ?? null,
            HeadingDegrees: geo?.heading ?? null,
            TimestampEpochMs: geo?.ts ?? null,
            City: geo?.city ?? null,

            EnvBrowser: env?.browser ?? null,
            EnvBrowserVersion: env?.browserVersion ?? null,
            EnvOperatingSystem: env?.operatingSystem ?? null,
            EnvOSVersion: env?.osVersion ?? null,
            EnvArchitecture: env?.architecture ?? null,
            EnvDeviceType: env?.deviceType ?? null,
            EnvDeviceModel: env?.deviceModel ?? null,
            EnvTouchPoints: env?.touchPoints ?? null,
            EnvIsBot: env?.isBot ?? null,
            EnvBotName: env?.botName ?? null,
            EnvLanguage: env?.language ?? null,
            EnvLanguagesJson: env?.languages ?? null, // objeto/array → jsonb
            EnvPlatform: env?.platform ?? null,
            EnvIsOnline: env?.online ?? null,
            EnvTimeZone: env?.timeZone ?? null,
            EnvScreenWidth: env?.screenWidth ?? null,
            EnvScreenHeight: env?.screenHeight ?? null,
            EnvDevicePixelRatio: env?.dpr ?? null,
            EnvReferrer: env?.referrer ?? null,
            EnvPageUrl: env?.page ?? null,

            NetDownlink: env?.connection?.downlink ?? null,
            NetEffectiveType: env?.connection?.effectiveType ?? null,
            NetRtt: env?.connection?.rtt ?? null,
            NetSaveData: env?.connection?.saveData ?? null,

            Error: error ?? null,

            CorrelationId: correlationId,
            SessionId: getOrCreateSessionId()
        };
    }

    async function postLog(payload) {
        try {
            if (PREFLIGHT_DIAGNOSTIC) {
                const ok = await preflightCheck(GEO_ENDPOINT);
                if (!ok) throw new Error("Preflight CORS falhou");
            }
            const res = await postWithRetry(GEO_ENDPOINT, payload, POST_RETRIES);
            setIndicator("api-status-indicator", true, `API OK: ${GEO_ENDPOINT}`, "");
            return res;
        } catch (err) {
            setIndicator("api-status-indicator", false, "", `Erro API: ${GEO_ENDPOINT}\n${err.message || err}`);
            throw err;
        }
    }

    // ==========================
    // Init (1 request por page load)
    // ==========================
    function validateApiBase() {
        try {
            const u = new URL(API_BASE);
            if (/^\d+\.\d+\.\d+\.\d+$/.test(u.hostname)) {
                derr("API_BASE aponta para IP; prefira hostname com certificado válido:", API_BASE);
            }
        } catch { /* ignore */ }
    }

    async function init() {
        if (started) return;
        started = true;
        dlog("init start", { API_BASE, GEO_ENDPOINT });
        validateApiBase();

        const env = await getEnvInfoAsync();
        const cached = readCache();

        // Sempre faz um único POST (usa cache se existir)
        if (cached?.coords) {
            setDom({ geo: cached.coords, env });
            try {
                await postLog(toPascalPayload(cached.coords, env, null));
            } catch (e) {
                derr("postLog (cached) failed", e);
            }
        } else {
            try {
                const pos = await getPosition();
                const place = null; // se tiver reverse geocoding, preencha
                const geo = pickAllFieldsFrom(pos, place);
                writeCache(geo, place);
                setDom({ geo, env });
                await postLog(toPascalPayload(geo, env, null));
            } catch (err) {
                derr("Geolocation failed", err?.message || err);
                try {
                    await postLog(toPascalPayload(null, env, err?.message || String(err)));
                } catch (e2) {
                    derr("postLog (error path) failed", e2);
                }
            }
        }

        // Health opcional
        // try { await checkDbStatus(); } catch {}
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", () => { init(); }, { once: true });
    } else {
        init();
    }

    // Exposição (debug)
    window.addEventListener("fsi:geo", (e) => dlog("ready (event)", e.detail));
})();

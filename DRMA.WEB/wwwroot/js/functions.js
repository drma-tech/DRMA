"use strict";

//function sendLog(msg) {
//    const baseUrl = window.location.hostname === "localhost" ? "http://localhost:7071" : "";

//    fetch(`${baseUrl}/api/public/logger`, {
//        method: "POST",
//        headers: { "Content-Type": "application/json" },
//        body: msg
//    }).catch(() => { /* do nothing */ });
//}

function jsSaveAsFile(filename, contentType, content) {
    // Create the URL
    const file = new File([content], filename, { type: contentType });
    const exportUrl = URL.createObjectURL(file);

    // Create the <a> element and click on it
    const a = document.createElement("a");
    document.body.appendChild(a);
    a.href = exportUrl;
    a.download = filename;
    a.target = "_self";
    a.click();

    // We don't need to keep the object URL, let's release the memory
    // On older versions of Safari, it seems you need to comment this line...
    URL.revokeObjectURL(exportUrl);
}

function GetLocalStorage(key) {
    return window.localStorage.getItem(key);
}

function SetLocalStorage(key, value) {
    if (typeof key !== "string" || typeof value !== "string") {
        showError("Key/value must be strings");
        return null;
    }
    return window.localStorage.setItem(key, value);
}

function LoadAppVariables() {
    //platform
    if (!GetLocalStorage("platform")) {
        const isWindows = document.referrer == "app-info://platform/microsoft-store" || /microsoft-store/i.test(navigator.userAgent);
        const isAndroid = /android/i.test(navigator.userAgent);
        const isIOS = /iphone|ipad|ipod/i.test(navigator.userAgent);
        const isMac = /macintosh|mac os x/i.test(navigator.userAgent);
        const isHuawei = /huawei|honor/i.test(navigator.userAgent);
        const isXiaomi = /xiaomi/i.test(navigator.userAgent);

        if (isWindows)
            SetLocalStorage("platform", "windows");
        else if (isAndroid)
            SetLocalStorage("platform", "play");
        else if (isIOS || isMac)
            SetLocalStorage("platform", "ios");
        else if (isHuawei)
            SetLocalStorage("platform", "huawei");
        else if (isXiaomi)
            SetLocalStorage("platform", "xiaomi");
        else
            SetLocalStorage("platform", "webapp");
    }
}

//async function getUserInfo() {
//    try {
//        if (window.location.host.includes("localhost")) {
//            const response = await fetch("/dev-env/me.json");
//            if (!response.ok) throw new Error(`HTTP ${response.status}`);
//            const userInfo = await response.json();
//            return userInfo?.clientPrincipal;
//        }
//        else {
//            const response = await fetch("/.auth/me");
//            if (!response.ok) throw new Error(`HTTP ${response.status}`);
//            const userInfo = await response.json();
//            return userInfo?.clientPrincipal;
//        }
//    } catch (error) {
//        showError(error.message);
//        return null;
//    }
//}

function showError(message) {
    if (window.DotNet) {
        try {
            DotNet.invokeMethodAsync("SD.WEB", "ShowError", message);
        }
        catch {
            showToast(message);
        }
    }
    else {
        showToast(message);
    }
}

function showToast(message) {
    const container = document.getElementById("error-container");
    if (!container) return;

    container.textContent = message;
    container.style.display = "block";

    setTimeout(() => {
        container.style.display = "none";
    }, 10000);
}

async function detectBrowserFeatures() {
    const [simd, bulkMemory, bigInt] = await Promise.all([
        wasmFeatureDetect.simd(),
        wasmFeatureDetect.bulkMemory(),
        wasmFeatureDetect.bigInt()
    ]);

    return simd && bulkMemory && bigInt;
}

function showBrowserWarning() {
    document.body.innerHTML = `
        <div style="display:flex; align-items:center; justify-content:center; min-height:100vh; background:#f0f2f5; font-family:'Segoe UI', Roboto, sans-serif; padding:1rem;">
            <div style="background:#fff; padding:1.2rem; border-radius:16px; box-shadow:0 4px 12px rgba(0,0,0,0.1); width:100%; max-width:380px; text-align:center; color:#333;">
                <div style="font-size:2.2rem; margin-bottom:0.5rem;">⚠️</div>
                <h2 style="font-size:1.3rem; margin-bottom:0.75rem;">Your browser is too old</h2>
                <p style="font-size:1rem; line-height:1.5; margin-bottom:1rem;">
                    This app can’t run on your current browser because it is out of date.
                    Please update your device using the instructions below.
                </p>
                <div style="text-align:left; font-size:1rem; color:#444;">
                    <div style="margin:0.8rem 0; display:flex; align-items:center;">
                        <img src="https://cdn.jsdelivr.net/npm/simple-icons@v11/icons/googleplay.svg" width="22" style="margin-right:8px;" />
                        <span><strong>Android:</strong> update <strong>Google Chrome</strong> in the Play Store</span>
                    </div>
                    <div style="margin:0.8rem 0; display:flex; align-items:center;">
                        <img src="https://cdn.jsdelivr.net/npm/simple-icons@v11/icons/apple.svg" width="22" style="margin-right:8px;" />
                        <span><strong>iOS / macOS:</strong> update your system (Safari is included)</span>
                    </div>
                    <div style="margin:0.8rem 0; display:flex; align-items:center;">
                        <img src="https://cdn.jsdelivr.net/npm/simple-icons@v11/icons/windows.svg" width="22" style="margin-right:8px;" />
                        <span><strong>Windows:</strong> run Windows Update (Edge is included)</span>
                    </div>
                </div>
                <p style="font-size:0.9rem; color:#777; margin-top:1.2rem; text-align:center;">
                    If you cannot update, try opening this app on a newer device.
                </p>
            </div>
        </div>
    `;
}

function getBrowserName() {
    const ua = navigator.userAgent;
    if (ua.includes("Firefox/")) return "Firefox";
    if (ua.includes("Edg/")) return "Edge";
    if (ua.includes("Chrome/")) return "Chrome";
    if (ua.includes("Safari/")) return "Safari";
    if (ua.includes("OPR/")) return "Opera";
    if (ua.includes("MSIE") || ua.includes("Trident/")) return "Internet Explorer";
    return "Unknown";
}

function getBrowserVersion() {
    const ua = navigator.userAgent;
    const matches = RegExp(/(Firefox|Edg|Chrome|Safari|Version)\/([0-9.]+)/).exec(ua);
    return matches ? matches[2] : "unknown";
}

function getOperatingSystem() {
    const ua = navigator.userAgent;
    if (ua.includes("Windows")) return "Windows";
    if (ua.includes("Mac")) return "Mac OS";
    if (ua.includes("Linux")) return "Linux";
    if (ua.includes("Android")) return "Android";
    if (ua.includes("iOS") || ua.includes("iPhone") || ua.includes("iPad")) return "iOS";
    return "Unknown";
}

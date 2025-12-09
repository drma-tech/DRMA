"use strict";

import { isBot, baseApiUrl } from "./main.js";
import { simd } from "./wasm-feature-detect.js";

export const storage = {
    clearLocalStorage() {
        localStorage.clear();

        if (window.WTN.clearAppCache) {
            window.WTN.clearAppCache(true);
        }
    },
    getLocalStorage(key) {
        return localStorage.getItem(key);
    },
    getSessionStorage(key) {
        return sessionStorage.getItem(key);
    },
    setLocalStorage(key, value) {
        if (typeof key !== "string" || typeof value !== "string") {
            notification.showError("Key/value must be strings");
            return null;
        }
        return localStorage.setItem(key, value);
    },
    setSessionStorage(key, value) {
        if (typeof key !== "string" || typeof value !== "string") {
            notification.showError("Key/value must be strings");
            return null;
        }
        return sessionStorage.setItem(key, value);
    },
    showCache() {
        notification.showToast(
            `userAgent: ${navigator.userAgent},
            app-language: ${this.getLocalStorage("app-language")},
            app-version: ${this.getLocalStorage("app-version")},
            country: ${this.getLocalStorage("country")},
            platform: ${this.getLocalStorage("platform")}`
        );
    },
};

export const notification = {
    showError(message) {
        if (window.DotNet) {
            try {
                window.DotNet.invokeMethodAsync("SD.WEB", "ShowError", message);
            } catch {
                this.showToast(message);
            }
        } else {
            this.showToast(message);
        }
    },
    showToast(message, attempts = 20) {
        const stack = document.getElementById("toast-stack");
        if (!stack) return;

        if (!stack) {
            if (attempts > 0) {
                setTimeout(() => {
                    this.showToast(message, attempts - 1);
                }, 1000);
            } else {
                console.warn("showToast: error-container not found");
            }
            return;
        }

        const exists = Array.from(stack.children).some(
            (el) => el.textContent === message
        );
        if (exists) return;

        const toast = document.createElement("div");
        toast.className = "toast";
        toast.textContent = message;

        stack.appendChild(toast);

        setTimeout(() => {
            toast.remove();
        }, 10000);
    },
    sendLog(error) {
        let log;
        if (error instanceof Error) {
            log = {
                Message: error.message,
                StackTrace: error.stack,
                Origin: `instanceof Error - name:${error.name || "unknown"}|url:${location.href}`,
                OperationSystem: environment.getOperatingSystem(),
                BrowserName: environment.getBrowserName(),
                BrowserVersion: environment.getBrowserVersion(),
                Platform: storage.getLocalStorage("platform"),
                AppVersion: storage.getLocalStorage("app-version"),
                UserAgent: navigator.userAgent,
            };
        } else if (typeof error === "object") {
            log = error;
        } else if (typeof error === "string") {
            log = {
                Message: error,
                Origin: `string - url:${location.href}`,
                OperationSystem: environment.getOperatingSystem(),
                BrowserName: environment.getBrowserName(),
                BrowserVersion: environment.getBrowserVersion(),
                Platform: storage.getLocalStorage("platform"),
                AppVersion: storage.GetLocalStorage("app-version"),
                UserAgent: navigator.userAgent,
            };
        } else {
            notification.showError("sendLog: invalid error type");
            return;
        }

        fetch(`${baseApiUrl}/api/public/logger`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(log),
        }).catch(() => {
            notification.showError("sendLog: failed to send log.");
        });
    },
    showBrowserWarning() {
        const os = environment.getOperatingSystem();
        const browser = environment.getBrowserName();
        const version = environment.getBrowserVersion();

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
                <div style="background:#f9fafb; border-radius:12px; padding:0.8rem; font-size:0.95rem; color:#444; margin-bottom:1rem;">
                    <strong>Detected environment:</strong><br>
                    ${os}<br>
                    ${browser} ${version}
                </div>
                <p style="font-size:0.9rem; color:#777; margin-top:1.2rem; text-align:center;">
                    If you cannot update, try opening this app on a newer device.
                </p>
            </div>
        </div>
    `;
    },
    playBeep(frequency, duration, type) {
        try {
            const audioCtx = new (window.AudioContext ||
                window.webkitAudioContext)();
            const oscillator = audioCtx.createOscillator();
            const gainNode = audioCtx.createGain();

            oscillator.type = type; // "sine", "square", "triangle", "sawtooth"
            oscillator.frequency.setValueAtTime(
                frequency,
                audioCtx.currentTime
            );
            gainNode.gain.setValueAtTime(0.1, audioCtx.currentTime);

            oscillator.connect(gainNode);
            gainNode.connect(audioCtx.destination);

            oscillator.start();
            oscillator.stop(audioCtx.currentTime + duration / 1000);
        } catch (err) {
            console.warn("Audio playback failed:", err);
        }
    },
    vibrate(pattern) {
        if (navigator.vibrate) navigator.vibrate(pattern);
    },
};

export const environment = {
    detectPlatform() {
        if (!storage.getLocalStorage("platform")) {
            const isWindows = document.referrer === "app-info://platform/microsoft-store";
            const isAndroid = /(android)/i.test(navigator.userAgent);
            const isIOS = /iphone|ipad|ipod/i.test(navigator.userAgent);
            const isMac = /macintosh|mac os x/i.test(navigator.userAgent);
            const isHuawei = /huawei|honor/i.test(navigator.userAgent); //not working. returns play
            const isXiaomi = /xiaomi/i.test(navigator.userAgent); //not working. returns play

            if (isWindows) storage.setLocalStorage("platform", "windows");
            else if (isAndroid) storage.setLocalStorage("platform", "play");
            else if (isIOS || isMac) storage.setLocalStorage("platform", "ios");
            else if (isHuawei) storage.setLocalStorage("platform", "huawei");
            else if (isXiaomi) storage.setLocalStorage("platform", "xiaomi");
            else storage.setLocalStorage("platform", "webapp");
        }
    },
    async checkBrowserFeatures() {
        const wasmSupported = typeof WebAssembly === "object";
        const simdSupported = await simd();

        if (!wasmSupported || !simdSupported) {
            if (!wasmSupported) {
                notification.showBrowserWarning();
                return;
            }

            if (!simdSupported) {
                notification.showError(
                    "Your browser is out of date or some security mechanism is blocking something essential for the platform to function properly, such as Edge's Enhanced Security Mode."
                );
                return;
            }
        }
    },
    getBrowserName() {
        const ua = navigator.userAgent;
        if (ua.includes("Firefox/")) return "Firefox";
        if (ua.includes("Edg/")) return "Edge";
        if (ua.includes("Chrome/")) return "Chrome";
        if (ua.includes("Safari/")) return "Safari";
        if (ua.includes("OPR/")) return "Opera";
        if (ua.includes("MSIE") || ua.includes("Trident/")) return "Internet Explorer";
        return "Unknown";
    },
    getBrowserVersion() {
        const ua = navigator.userAgent;
        const matches = new RegExp(
            /(Firefox|Edg|Chrome|Safari|Version)\/([0-9.]+)/
        ).exec(ua);
        return matches ? matches[2] : "unknown";
    },
    getOperatingSystem() {
        const ua = navigator.userAgent;
        if (ua.includes("Windows")) return "Windows";
        if (ua.includes("Mac")) return "Mac OS";
        if (ua.includes("Linux")) return "Linux";
        if (ua.includes("Android")) return "Android";
        if (ua.includes("iOS") || ua.includes("iPhone") || ua.includes("iPad")) return "iOS";
        return "Unknown";
    },
};

export const interop = {
    downloadFile(filename, contentType, content) {
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
    },
    async invokeDotNetWhenReady(assembly, method, args) {
        const maxRetries = 25;
        let delay = 300;
        const delayStep = 300;
        const maxDelay = 5000;

        for (let i = 0; i < maxRetries; i++) {
            if (window.DotNet?.invokeMethodAsync) {
                try {
                    await window.DotNet.invokeMethodAsync(
                        assembly,
                        method,
                        args
                    );
                    return;
                } catch {
                    //ignores
                }
            }
            await new Promise((resolve) => setTimeout(resolve, delay));
            delay = Math.min(delay + delayStep, maxDelay);
        }
        console.error(
            `DotNet not ready after multiple retries. method: ${method}`
        );
    },
};

if (!isBot) {
    environment.detectPlatform();
    environment.checkBrowserFeatures();
}

window.checkUpdateReady = async function () {
    if (!navigator.serviceWorker) return false;
    const reg = await navigator.serviceWorker.ready;
    return !!reg.waiting;
};

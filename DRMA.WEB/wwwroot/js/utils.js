"use strict";

import { isBot, isOldBrowser, appVersion, baseApiUrl } from "./main.js";
import { simd } from "./wasm-feature-detect.js";

export const storage = {
    clearAllStorage() {
        localStorage.clear();
        sessionStorage.clear();

        if (window.WTN?.clearAppCache) {
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
        localStorage.setItem(key, value);
    },
    setSessionStorage(key, value) {
        if (typeof key !== "string" || typeof value !== "string") {
            notification.showError("Key/value must be strings");
            return null;
        }
        sessionStorage.setItem(key, value);
    },
    removeLocalStorage(key) {
        localStorage.removeItem(key);
    },
    removeSessionStorage(key) {
        sessionStorage.removeItem(key);
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
                window.DotNet.invokeMethodAsync("DRMA.WEB", "ShowError", message);
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
                AppVersion: appVersion,
                Country: storage.getLocalStorage("country"),
                UserAgent: navigator.userAgent,
                IsBot: isBot || isOldBrowser,
            };
        } else if (typeof error === "string") {
            log = {
                Message: error,
                Origin: `string - url:${location.href}`,
                OperationSystem: environment.getOperatingSystem(),
                BrowserName: environment.getBrowserName(),
                BrowserVersion: environment.getBrowserVersion(),
                Platform: storage.getLocalStorage("platform"),
                AppVersion: appVersion,
                Country: storage.getLocalStorage("country"),
                UserAgent: navigator.userAgent,
                IsBot: isBot || isOldBrowser,
            };
        } else {
            log = error;
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
            <div style="background:#fff; padding:1.2rem; border-radius:16px; box-shadow:0 4px 12px rgba(0,0,0,0.1); width:100%; max-width:450px; text-align:center; color:#333;">
                <div style="font-size:2.2rem; margin-bottom:0.5rem;">⚠️</div>
                <h2 style="font-size:1.3rem; margin-bottom:0.75rem;">Your browser is too old</h2>
                <p style="font-size:1rem; line-height:1.5; margin-bottom:1rem;">
                    This app cannot run in your current browser because it is out of date. Update your device by following the instructions below:
                </p>
                <div style="text-align:left; font-size:1rem; color:#444;">
                    <div style="margin:0.8rem 0; display:flex; align-items:center;">
                        <img src="logo/google-play.png" width="22" style="margin-right:8px;" />
                        <span><strong>Android:</strong> update <strong>Google Chrome</strong> in the Play Store</span>
                    </div>
                    <div style="margin:0.8rem 0; display:flex; align-items:center;">
                          <img src="logo/app-store.png" width="22" style="margin-right:8px;" />
                        <span><strong>iOS / macOS:</strong> update your system (Safari is included)</span>
                    </div>
                    <div style="margin:0.8rem 0; display:flex; align-items:center;">
                        <img src="logo/microsoft-store.png" width="22" style="margin-right:8px;" />
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
            const isHuawei = /huawei|honor/i.test(navigator.userAgent); //not working. returns play
            const isXiaomi = /xiaomi/i.test(navigator.userAgent); //not working. returns play
            const isAndroid = /(android)/i.test(navigator.userAgent);
            const isIOS = /iphone|ipad|ipod/i.test(navigator.userAgent);
            const isMac = /macintosh|mac os x/i.test(navigator.userAgent);

            if (isWindows) storage.setLocalStorage("platform", "windows");
            else if (isHuawei) storage.setLocalStorage("platform", "huawei");
            else if (isXiaomi) storage.setLocalStorage("platform", "xiaomi");
            else if (isAndroid) storage.setLocalStorage("platform", "play");
            else if (isIOS || isMac) storage.setLocalStorage("platform", "ios");
            else storage.setLocalStorage("platform", "webapp");
        }
    },
    async checkBrowserFeatures() {
        const wasmSupported = typeof WebAssembly === "object";

        //isOldBrowser - its probably a bot with fake user agent
        if (!wasmSupported || isOldBrowser) {
            notification.showBrowserWarning();
            return;
        }

        const simdSupported = await simd();

        if (!simdSupported) {
            notification.showError(
                "Your browser is out of date or some security mechanism is blocking something essential for the platform to function properly, such as Edge's Enhanced Security Mode."
            );
        }
    },
    getBrowserName() {
        return window.browser.getBrowserName();
    },
    getBrowserVersion() {
        return window.browser.getBrowserVersion();
    },
    getOperatingSystem() {
        return window.browser.getOSName();
    },
    getAppVersion() {
        return appVersion;
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

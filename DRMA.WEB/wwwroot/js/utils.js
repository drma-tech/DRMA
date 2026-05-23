"use strict";

import { isBot, hideBlazorIndex } from "./main.js";
import { simd } from "./wasm-feature-detect.js";
import { appVersion } from "./app-version.js";

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
                        <img src="logo/google-play.png" width="22" style="margin-right:8px;" alt="Google Play" />
                        <span><strong>Android:</strong> update <strong>Google Chrome</strong> in the Play Store</span>
                    </div>
                    <div style="margin:0.8rem 0; display:flex; align-items:center;">
                        <img src="logo/app-store.png" width="22" style="margin-right:8px;" alt="App Store" />
                        <span><strong>iOS / macOS:</strong> update your system (Safari is included)</span>
                    </div>
                    <div style="margin:0.8rem 0; display:flex; align-items:center;">
                        <img src="logo/microsoft-store.png" width="22" style="margin-right:8px;" alt="Microsoft Store" />
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
            const audioCtx = new (window.AudioContext || window.webkitAudioContext)();
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
        let platform = storage.getLocalStorage("platform");

        //for some reason, sometimes platform is not getting the correct value on windows
        if (document.referrer === "app-info://platform/microsoft-store" && platform === "webapp") {
            platform = "windows";
            storage.setLocalStorage("platform", platform);
            return;
        }

        if (platform) return; //if its already detected, exit

        const ua = navigator.userAgent.toLowerCase();
        platform = "webapp"; //default value

        if (document.referrer === "app-info://platform/microsoft-store") {
            // Microsoft Store PWA
            platform = "windows";
        } else if (/huawei|honor|hisilicon|kirin/i.test(ua)) {
            // Huawei / Honor (model in UA after 2019)
            platform = "huawei";
        } else if (/xiaomi|miui|redmi|poco/i.test(ua)) {
            // Xiaomi (MIUI Browser + standard model)
            platform = "xiaomi";
        } else if (/kindle|silk|kf[a-z]{2,4}/i.test(ua)) {
            // Amazon Kindle / Fire (Silk Browser + model)
            platform = "amazon";
        } else if (/android/.test(ua)) {
            // Generic Android (last one so as not to overwrite the ones above)
            platform = "play";
        } else if (/iphone|ipad|ipod|macintosh|mac os x/i.test(ua)) {
            // iOS + macOS
            platform = "ios";
        }

        storage.setLocalStorage("platform", platform);
    },
    async validateBrowserAndPlatform() {
        const wasmSupported = typeof WebAssembly === "object";

        //The browser does not support WASM or SIMD.
        if (!wasmSupported || hideBlazorIndex) {
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
        return window.browser?.getBrowserName() ?? "no bowser loaded";
    },
    getBrowserVersion() {
        return window.browser?.getBrowserVersion() ?? "no bowser loaded";
    },
    getOperatingSystem() {
        return window.browser?.getOSName() ?? "no bowser loaded";
    },
    getAppVersion() {
        return appVersion;
    },
    inspectAdElement(el) {
        if (!el) return { rendered: false, hasSize: false };

        const iframe = el.querySelector('iframe');
        const rect = el.getBoundingClientRect();

        const rendered = !!iframe;
        const hasSize = rect.width > 0 && rect.height > 0;

        return { rendered, hasSize };
    },
    async waitForAds(els, timeout = 10000) {
        const start = Date.now();

        return new Promise((resolve) => {
            const interval = setInterval(() => {
                for (const el of els) {
                    const { rendered, hasSize } = environment.inspectAdElement(el);

                    if (rendered && hasSize) {
                        clearInterval(interval);
                        return resolve('filled');
                    }
                }

                if (Date.now() - start > timeout) {
                    clearInterval(interval);
                    resolve('suspected_blocked');
                }
            }, 200);
        });
    },
    testUrl(url) {
        return new Promise((resolve) => {
            let done = false;

            const script = document.createElement('script');

            const finish = (result) => {
                if (done) return;
                done = true;
                script.remove();
                resolve(result);
            };

            script.src = url;
            script.onload = () => finish(true);
            script.onerror = () => finish(false);

            document.head.appendChild(script);

            setTimeout(() => finish(false), 3000);
        });
    },
    async isAdBlocked() {
        if (window.location.hostname === 'localhost') { return false; }
        if (isBot) return false;
        if (hideBlazorIndex) return false;

        //detect if adsense exists
        const els = document.querySelectorAll('.adsbygoogle');
        if (!els.length) {
            Sentry.captureMessage("ad blocked - no .adsbygoogle elements found", "error");
            return false;
        }

        const state = await environment.waitForAds(els);

        if (state === 'filled') {
            return false;
        }

        //if not filled, test if adsbygoogle can be loaded
        const googlesyndication = await environment.testUrl(
            'https://pagead2.googlesyndication.com/pagead/js/adsbygoogle.js'
        );

        if (!googlesyndication) {
            return true;
        }

        //test browser brave as adsbygoogle works normally
        const isBrave = navigator.brave && typeof navigator.brave.isBrave === 'function' && await navigator.brave.isBrave();

        if (isBrave) {
            const fundingchoicesmessages =
                await environment.testUrl(
                    'https://fundingchoicesmessages.google.com/i/pub-5145928155833172?ers=1'
                );

            if (!fundingchoicesmessages) {
                return true;
            }
        }

        Sentry.captureMessage("ad blocked - Ads failed but no blocker detected", "error");
        return false;
    }
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
    async invokeDotNetWhenReady(assembly, method, args, maxRetries = 20, initialDelay = 300, maxDelay = 2000) {
        let delay = initialDelay;

        for (let attempt = 1; attempt <= maxRetries; attempt++) {
            if (window.DotNet?.invokeMethodAsync) {
                try {
                    await window.DotNet.invokeMethodAsync(assembly, method, args);
                    return;
                } catch {
                    //ignores -> No call dispatcher has been set.
                }
            }
            await new Promise((resolve) => setTimeout(resolve, delay));
            delay = Math.min(delay * 1.5, maxDelay);
        }

        throw new Error(`Blazor runtime never ready to receive JSInvokable ${method} from assembly ${assembly}`);
    },
    async share(title, text, url) {
        if (navigator.share) {
            await navigator.share({
                title: title,
                text: text,
                url: url
            });
            return true;
        }

        return false;
    }
};

if (!isBot) {
    environment.detectPlatform();
    environment.validateBrowserAndPlatform();
}

window.checkUpdateReady = async function () {
    if (!navigator.serviceWorker) return false;
    const reg = await navigator.serviceWorker.ready;
    return !!reg.waiting;
};

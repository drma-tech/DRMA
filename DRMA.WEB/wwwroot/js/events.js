"use strict";

import { notification } from "./utils.js";

//setTimeout(() => { throw new Error('error test call'); }, 100);

window.addEventListener("error", function (event) {
    const { message, filename } = event;

    if (filename?.includes("blazor.webassembly")) {
        notification.showBrowserWarning();
        return;
    }

    notification.showError(`error: ${message}`);
});

//Promise.reject(new Error('unhandledrejection test call'));

function normalizeReason(reason) {
    if (reason instanceof Error) {
        const props = [
            "message",
            "stack",
            "code",
            "constraint",
            "constraintName",
            "target",
        ];
        const extra = props
            .filter((p) => reason[p] && p !== "message")
            .map(
                (p) =>
                    `${p}: ${typeof reason[p] === "object" ? JSON.stringify(reason[p]) : reason[p]}`
            )
            .join(", ");

        return {
            message:
                reason?.message || reason?.name + (extra ? ` (${extra})` : ""),
            stack: reason.stack || "No stack trace",
        };
    }

    if (typeof reason === "string") {
        return {
            message: reason,
            stack: "No stack trace",
        };
    }

    let serialized;
    try {
        serialized = JSON.stringify(reason);
    } catch {
        serialized = "[Unserializable reason]";
    }

    return {
        message: serialized || "Unknown error",
        stack: reason.stack || "No stack trace",
    };
}

window.addEventListener("unhandledrejection", function (event) {
    const { message } = normalizeReason(event.reason);

    if (message.includes("Failed to fetch")) {
        notification.showError(
            "Unable to load required components. This may be a connection issue or a browser restriction. Try reopening the app."
        );
        //todo: study if this makes sense
        //setTimeout(() => {
        //    resetPwaAndReload();
        //}, 2000);
        return;
    }

    if (message.includes("Assert failed: .NET runtime")) {
        notification.showError("A fatal error occurred. Reloading the app...");
        setTimeout(() => location.reload(), 3000);
        return;
    }

    notification.showError(`unhandledrejection: ${message}`);
});

//async function resetPwaAndReload() {
//    try {
//        if ('caches' in window) {
//            const keys = await caches.keys();
//            await Promise.all(keys.map(key => caches.delete(key)));
//        }

//        if ('serviceWorker' in navigator) {
//            const registrations = await navigator.serviceWorker.getRegistrations();
//            await Promise.all(registrations.map(r => r.unregister()));
//        }
//    } catch (e) {
//        console.error("Reset failed", e);
//    }

//    location.reload(true);
//}

window.addEventListener("securitypolicyviolation", (event) => {
    const obj = {
        violatedDirective: event.violatedDirective,
        blockedURI: event.blockedURI,
        sourceFile: event.sourceFile,
        lineNumber: event.lineNumber,
        url: location.href,
    };

    Sentry.captureMessage(`securitypolicyviolation: ${JSON.stringify(obj)}`, "error");
});

let resizeTimeout;
window.addEventListener("resize", function () {
    clearTimeout(resizeTimeout);
    resizeTimeout = setTimeout(function () {
        const divs = document.querySelectorAll('[id^="swiper-trailer-"]');
        divs.forEach(function (el) {
            if (window.initGrid) {
                window.initGrid(el.id);
            }
        });
    }, 250);
});

const OFFLINE_COOLDOWN_MS = 5 * 60 * 1000; // 5 minutes cooldown
let lastOfflineNotifyAt = 0;
window.addEventListener("offline", () => {
    const now = Date.now();

    if (now - lastOfflineNotifyAt < OFFLINE_COOLDOWN_MS) return;

    notification.showError(
        "It looks like you're offline. Please check your connection."
    );

    lastOfflineNotifyAt = Date.now();
});
"use strict";

import { isDev, isBot, isOldBrowser, appVersion } from "./main.js";
import { storage, notification, environment } from "./utils.js";

window.addEventListener("load", function () {
    const startTime = performance.now();
    const app = document.getElementById("app");
    const messageEl = document.querySelector(".loading-message");

    if (app) {
        const checkConnection = setInterval(() => {
            const elapsed = (performance.now() - startTime) / 1000;
            const progress = Number.parseFloat(
                getComputedStyle(document.documentElement).getPropertyValue(
                    "--blazor-load-percentage"
                ) || "0"
            );

            // Clear previous classes
            app.classList.remove(
                "slow-connection",
                "very-slow-connection",
                "extremely-slow-connection"
            );

            if (elapsed > 80 && progress < 100) {
                app.classList.add("extremely-slow-connection");
                messageEl.textContent =
                    "Still loading. Something may be holding things up.";
            } else if (elapsed > 50 && progress < 100) {
                app.classList.add("very-slow-connection");
                messageEl.textContent =
                    "Still loading. This may take a little longer.";
            } else if (elapsed > 20 && progress < 100) {
                app.classList.add("slow-connection");
                messageEl.textContent =
                    "This is taking a bit longer than expected.";
            }

            if (progress >= 100) clearInterval(checkConnection);
        }, 1000);
    }
});

//setTimeout(() => { throw new Error('error test call'); }, 100);

window.addEventListener("error", function (event) {
    const { message, filename, lineno, colno, error } = event;

    if (filename?.includes("blazor.webassembly.js")) {
        notification.showBrowserWarning();
        return;
    }

    notification.showError(`error: ${message}`);

    const log = {
        Message: `event.message:${message}|error.message:${error?.message}`,
        StackTrace: error?.stack,
        Origin: `event error - filename:${filename}|url:${location.href}|lineno:${lineno}|colno:${colno}`,
        OperationSystem: environment.getOperatingSystem(),
        BrowserName: environment.getBrowserName(),
        BrowserVersion: environment.getBrowserVersion(),
        Platform: storage.getLocalStorage("platform"),
        AppVersion: appVersion,
        Country: storage.getLocalStorage("country"),
        UserAgent: navigator.userAgent,
        IsBot: isBot || isOldBrowser,
    };

    notification.sendLog(log);
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
    const { message, stack } = normalizeReason(event.reason);

    if (message.includes("Failed to fetch")) {
        if (!isDev) {
            notification.showError(
                "Connection problem detected. Check your internet connection and try reloading."
            );
            return;
        }
    }

    if (message.includes("Assert failed: .NET runtime")) {
        notification.showError("A fatal error occurred. Reloading the app...");
        setTimeout(() => location.reload(), 3000);
        return;
    }

    notification.showError(`unhandledrejection: ${message}`);

    const log = {
        Message: message,
        StackTrace: stack,
        Origin: `event unhandledrejection - url:${location.href}`,
        OperationSystem: environment.getOperatingSystem(),
        BrowserName: environment.getBrowserName(),
        BrowserVersion: environment.getBrowserVersion(),
        Platform: storage.getLocalStorage("platform"),
        AppVersion: appVersion,
        Country: storage.getLocalStorage("country"),
        UserAgent: navigator.userAgent,
        IsBot: isBot || isOldBrowser,
    };

    notification.sendLog(log);
});

window.addEventListener("securitypolicyviolation", (event) => {
    const obj = {
        violatedDirective: event.violatedDirective,
        blockedURI: event.blockedURI,
        sourceFile: event.sourceFile,
        lineNumber: event.lineNumber,
        url: location.href,
    };

    notification.sendLog(`securitypolicyviolation: ${JSON.stringify(obj)}`);
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

window.addEventListener("offline", () => {
    notification.showError(
        "It looks like you're offline. Please check your connection."
    );
});

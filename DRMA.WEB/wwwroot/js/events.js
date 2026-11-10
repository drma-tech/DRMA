"use strict";

import { notification } from "./utils.js";

//setTimeout(() => { throw new Error('error test call'); }, 100);

window.addEventListener("error", function (event) {
    const { message, filename, lineno, colno, error } = event;

    if (filename?.includes("blazor.webassembly")) {
        notification.showBrowserWarning();
        return;
    }

    if (message.includes("ResizeObserver loop completed with undelivered notifications")) {
        return;
    }

    if (message.includes("Failed to fetch")) {
        return;
    }

    if (message === "Script error.") {
        const details = {
            script_url: filename || null,
            line: lineno || null,
            column: colno || null,
            stack: error?.stack || null,
            page_url: window.location.href,
            referrer: document.referrer || null,
        };

        const hasDetails = details.script_url || details.stack;

        if (hasDetails) {
            Sentry?.captureMessage(`Script error: ${JSON.stringify(details)}`, "error");
        } else {
            Sentry?.captureMessage("Script error: details unavailable", {
                level: "error",
                tags: {
                    error_type: "cross_origin_script_error",
                },
                extra: details,
            });
        }

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
            message: reason?.message || reason?.name + (extra ? ` (${extra})` : ""),
            stack: reason?.stack || "No stack trace",
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
        stack: reason?.stack || "No stack trace",
    };
}

window.addEventListener("unhandledrejection", function (event) {
    const { message } = normalizeReason(event.reason);

    if (message.includes("Failed to fetch")) {
        notification.showError(
            "The app couldn't load the required components. This may be caused by your connection or browser settings. Try reopening the app."
        );

        return;
    }

    if (message.includes("Assert failed: .NET runtime")) {
        notification.showError("A critical error occurred. The app will reload shortly...");
        setTimeout(() => location.reload(), 3000);
        return;
    }

    notification.showError(`unhandledrejection: ${message}`);
});

window.addEventListener("securitypolicyviolation", (event) => {
    const obj = {
        violatedDirective: event.violatedDirective,
        effectiveDirective: event.effectiveDirective,
        blockedURI: event.blockedURI,
        sourceFile: event.sourceFile,
        lineNumber: event.lineNumber,
        columnNumber: event.columnNumber,
        originalPolicy: event.originalPolicy,
        disposition: event.disposition,
        documentURI: event.documentURI,
        statusCode: event.statusCode,
        url: location.href,
    };

    Sentry?.captureMessage(`securitypolicyviolation: ${JSON.stringify(obj)}`, "error");
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

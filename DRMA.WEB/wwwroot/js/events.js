"use strict";

window.addEventListener("load", function () {
    const startTime = performance.now();
    const app = document.getElementById("app");
    const messageEl = document.querySelector(".loading-message");

    if (app) {
        const checkConnection = setInterval(() => {
            const elapsed = (performance.now() - startTime) / 1000;
            const progress = parseFloat(getComputedStyle(document.documentElement)
                .getPropertyValue("--blazor-load-percentage") || "0");

            // Clear previous classes
            app.classList.remove("slow-connection", "very-slow-connection", "extremely-slow-connection");

            if (elapsed > 80 && progress < 100) {
                app.classList.add("extremely-slow-connection");
                messageEl.textContent = "Still loading. Something may be holding things up.";
            }
            else if (elapsed > 50 && progress < 100) {
                app.classList.add("very-slow-connection");
                messageEl.textContent = "Still loading. This may take a little longer.";
            }
            else if (elapsed > 20 && progress < 100) {
                app.classList.add("slow-connection");
                messageEl.textContent = "This is taking a bit longer than expected.";
            }

            if (progress >= 100) clearInterval(checkConnection);
        }, 1000);
    }
});

//setTimeout(() => { throw new Error('error test call'); }, 100);

window.addEventListener("error", function (event) {
    if (event.filename?.includes("blazor.webassembly.js")) {
        showBrowserWarning();
    }
    else {
        showError(event.message);

        //const errorInfo = {
        //    message: event.message,
        //    filename: event.filename,
        //    errorMessage: event.error.message,
        //    errorStack: event.error.stack,
        //    env: `${getOperatingSystem()} | ${getBrowserName()} | ${getBrowserVersion()}`,
        //    app: `${GetLocalStorage("platform")} | ${GetLocalStorage("app-version")}`,
        //    userAgent: navigator.userAgent,
        //    url: window.location.href
        //};

        //sendLog(`error: ${JSON.stringify(errorInfo)}`);
    }
});

//Promise.reject(new Error('unhandledrejection test call'));

window.addEventListener("unhandledrejection", function (event) {
    const reasonMessage = event.reason?.message || 'Unknown error';
    //const reasonStack = event.reason?.stack || 'No stack trace';

    if (reasonMessage.includes('Failed to fetch')) {
        showError("Connection problem detected. Check your internet connection and try reloading.");
        return;
    }

    showError(reasonMessage);

    //if (!/google|baidu|bingbot|duckduckbot|teoma|slurp|yandex/i.test(window.navigator.userAgent) && window.navigator.serviceWorker?.register) {
    //    //just ignore, just a bot
    //    return;
    //}

    //const obj = {
    //    reasonMessage: reasonMessage,
    //    reasonStack: reasonStack,
    //    env: `${getOperatingSystem()} | ${getBrowserName()} | ${getBrowserVersion()}`,
    //    app: `${GetLocalStorage("platform")} | ${GetLocalStorage("app-version")}`,
    //    userAgent: navigator.userAgent,
    //    url: window.location.href
    //};

    //sendLog(`unhandledrejection: ${JSON.stringify(obj)}`);
});

window.addEventListener("securitypolicyviolation", (event) => {
    showError(`securitypolicyviolation: violatedDirective: ${event.violatedDirective}, blockedURI: ${event.blockedURI}, sourceFile: ${event.sourceFile}`);

    //const obj = {
    //    violatedDirective: event.violatedDirective,
    //    blockedURI: event.blockedURI,
    //    sourceFile: event.sourceFile,
    //    lineNumber: event.lineNumber,
    //    env: `${getOperatingSystem()} | ${getBrowserName()} | ${getBrowserVersion()}`,
    //    app: `${GetLocalStorage("platform")} | ${GetLocalStorage("app-version")}`,
    //    url: window.location.href
    //};

    //sendLog(`securitypolicyviolation: ${JSON.stringify(obj)}`);
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

window.addEventListener('offline', () => {
    showError("It looks like you're offline. Please check your connection.");
});

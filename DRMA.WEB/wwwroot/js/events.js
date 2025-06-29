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

window.addEventListener("error", function (e) {
    if (e.filename?.includes("blazor.webassembly.js")) {
        showBrowserWarning();
    }
    else {
        showError(e.message);
        //todo: send log to server
    }
});

window.addEventListener("unhandledrejection", function (e) {
    showError(e.reason.message);
    //todo: send log to server
});

window.addEventListener("resize", function () {
    const divs = document.querySelectorAll('[id^="swiper-trailer-"]');
    divs.forEach(function (el) {
        if (window.initGrid) {
            window.initGrid(el.id);
        }
    });
});
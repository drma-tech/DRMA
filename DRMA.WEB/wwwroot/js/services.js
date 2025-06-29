"use strict";

// Google Analytics
window.initGoogleAnalytics = function (code) {
    window.dataLayer = window.dataLayer || [];
    function gtag() { dataLayer.push(arguments); }
    gtag("js", new Date());

    getUserInfo()
        .then(userId => {
            if (userId) {
                gtag("config", code, {
                    'user_id': userId
                });
            }
            else {
                gtag("config", code);
            }
        })
        .catch(error => {
            showError(error.message);
        });
};

// Microsoft Clarity
window.initClarity = function (code) {
    if (!window.location.host.includes("localhost")) {
        (function (c, l, a, r, i, t, y) {
            c[a] = c[a] || function () { (c[a].q = c[a].q || []).push(arguments) };
            t = l.createElement(r); t.async = 1; t.src = "https://www.clarity.ms/tag/" + i;
            y = l.getElementsByTagName(r)[0]; y.parentNode.insertBefore(t, y);
        })(window, document, "clarity", "script", code);

        // Check if Clarity is loaded and call the consent function
        const clarityCheckInterval = setInterval(function () {
            if (window.clarity) {
                window.clarity("consent");
                clearInterval(clarityCheckInterval);
            }
        }, 5000);
    }
};

// Disable robots for dev environment
window.setRobotsMeta = function () {
    const meta = document.createElement("meta");
    meta.name = "robots";
    meta.content = window.location.hostname.includes("dev") ? "noindex, nofollow" : "index";
    document.head.appendChild(meta);
};
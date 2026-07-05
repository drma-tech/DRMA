"use strict";

import { storage } from "./utils.js";

export const services = {
    initGoogleAnalytics(version) {
        if (window.appConfig.isLocalhost) return;
        if (window.appConfig.isDev) return;

        const PLATFORM = storage.getLocalStorage("platform");

        window.dataLayer = window.dataLayer || [];
        function gtag() {
            window.dataLayer.push(arguments);
        }
        gtag("js", new Date());

        const config = {
            app_version: version,
            platform: PLATFORM,
        };

        gtag("config", window.appConfig.servicesConfig.AnalyticsCode, config);
    },
    initMicrosoftClarity(code) {
        if (window.appConfig.isLocalhost) return;
        if (window.appConfig.isDev) return;

        (function (c, l, a, r, i, t, y) {
            c[a] = c[a] || function () { (c[a].q = c[a].q || []).push(arguments) };
            t = l.createElement(r); t.async = 1; t.src = "https://www.clarity.ms/tag/" + i;
            y = l.getElementsByTagName(r)[0]; y.parentNode.insertBefore(t, y);
        })(window, document, "clarity", "script", code);

        //todo: whem implement tracking consent, modify this to wait for user consent
        const clarityCheckInterval = setInterval(function () {
            if (window.clarity) {
                window.clarity("consent");
                clearInterval(clarityCheckInterval);
            }
        }, 5000);
    },
};

services.initMicrosoftClarity(window.appConfig.servicesConfig.ClarityKey);

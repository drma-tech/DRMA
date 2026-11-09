"use strict";

import { storage } from "./utils.js";

export const services = {
    initGoogleAnalytics(version) {
        if (window.appConfig?.isLocalhost === true) return;
        if (window.appConfig?.isDev === true) return;

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

        gtag("config", window.appConfig?.servicesConfig?.AnalyticsCode, config);
    },
    initMicrosoftClarity(code) {
        if (window.appConfig?.isLocalhost === true) return;
        if (window.appConfig?.isDev === true) return;

        (function (c, l, a, r, i, t, y) {
            c[a] = c[a] || function () { (c[a].q = c[a].q || []).push(arguments) };
            t = l.createElement(r); t.async = 1; t.src = "https://www.clarity.ms/tag/" + i;
            y = l.getElementsByTagName(r)[0]; y.parentNode.insertBefore(t, y);
        })(window, document, "clarity", "script", code);

        // TODO: Determine whether Clarity's cookies, session replay, and collected fields require prior consent in each browser/PWA jurisdiction; apply the relevant consent or objection controls before collection. For native wrappers, assess actual personal-information collection against the applicable Huawei mainland-China and Xiaomi GetApps rules instead of blocking every SDK by default. ATT is only for Apple's cross-company tracking. Store labels are disclosures, not consent.
        // TODO: Do not set analytics/ad storage to granted before required browser consent. Where consent is not the required legal basis, use the applicable lawful basis and honor objection/opt-out rights. Do not reuse browser consent defaults as a universal native-app rule. For native wrappers, apply Huawei mainland-China and Xiaomi GetApps pre-collection requirements only to covered personal-information flows; ATT remains limited to Apple's cross-company tracking. Support withdrawal or objection as applicable.
        let attempts = 0;
        const maxAttempts = 12;
        const clarityCheckInterval = setInterval(function () {
            if (window.clarity) {
                clearInterval(clarityCheckInterval);
                window.clarity("consentv2", {
                    ad_Storage: "granted",
                    analytics_Storage: "granted"
                });
                return;
            }

            attempts++;
            if (attempts >= maxAttempts) clearInterval(clarityCheckInterval);
        }, 5000);
    },
};

services.initMicrosoftClarity(window.appConfig?.servicesConfig?.ClarityKey);

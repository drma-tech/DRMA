"use strict";

import { storage, environment } from "./utils.js";

const env = (() => {
    if (window.appConfig?.isLocalhost === true) return "development";
    if (window.appConfig?.isDev === true) return "staging";
    return "production";
})();

const ignoredErrors = [
    /failed to fetch/i,
    /failed to register/i,
    /failed to start/i,
    /token has expired/i,
];

window.sentryOnLoad = function () {
    Sentry.init({
        dsn: window.appConfig?.servicesConfig?.SentryDsn,
        sendDefaultPii: true, // enable ip
        release: `drma-js@${window.appVersion}`,
        environment: env,
        ignoreErrors: ignoredErrors,
        beforeSend(event) {
            event.tags = {
                ...event.tags,
                "custom.version": window.appVersion,
                "custom.platform": storage.getLocalStorage("platform") ?? "unknown",
                "custom.isAdBlocked": window.isAdBlocked ?? "unknown",
                "custom.isAuthenticated": window.isAuthenticated ?? false,
                "custom.isBot": window.appConfig?.isBot ?? "unknown",
                "custom.blazorSupported": window.appConfig?.blazorSupported ?? "unknown",
                "custom.disableServiceWorker": window.appConfig?.disableServiceWorker ?? "unknown",
            };
            event.extra = {
                ...event.extra,
                browser_name: environment.getBrowserName() ?? "unknown",
                browser_version: environment.getBrowserVersion() ?? "unknown",
                operation_system: environment.getOperatingSystem() ?? "unknown",
            };

            return event;
        },
    });
}

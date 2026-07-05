import { storage, environment } from "./utils.js";

const env = (() => {
    if (window.appConfig.isLocalhost) return "development";
    if (window.appConfig.isDev) return "staging";
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
        dsn: window.appConfig.servicesConfig.SentryDsn,
        SendDefaultPii: true, // enable ip
        release: `drma-js@${window.appVersion}`,
        environment: env,
        beforeSend(event) {
            const message = event.exception?.values?.[0]?.value || event.message || "";

            if (message && ignoredErrors.some(err => err.test(message))) {
                return null;
            }

            event.tags = {
                "custom.version": window.appVersion,
                "custom.platform": storage.getLocalStorage("platform") ?? "error",
            };
            event.extra = {
                browser_name: environment.getBrowserName() ?? "error",
                browser_version: environment.getBrowserVersion() ?? "error",
                operation_system: environment.getOperatingSystem() ?? "error",
            };

            return event;
        },
    });
}

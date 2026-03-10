import { servicesConfig } from "./main.js";
import { storage, environment } from "./utils.js";
import { appVersion } from "./app-version.js";

const env = (() => {
    if (window.location.hostname === "localhost") return "development";
    if (window.location.hostname.startsWith("dev.")) return "staging";
    return "production";
})();

const ignoredErrors = [
    /failed to fetch/i,
    /failed to register/i,
    /failed to start/i,
    /wasm simd/i
];

Sentry.init({
    dsn: servicesConfig.SentryDsn,
    release: `drma-web@${appVersion}`,
    environment: env,
    beforeSend(event) {
        const exception = event.exception?.values?.[0];
        const message = exception?.value;

        if (message && ignoredErrors.some(err => err.test(message))) {
            return null;
        }

        event.tags = {
            "custom.version": appVersion,
            "custom.platform": storage.getLocalStorage("platform"),
        };
        event.extra = {
            browser_name: environment.getBrowserName(),
            browser_version: environment.getBrowserVersion(),
            operation_system: environment.getOperatingSystem(),
        };

        return event;
    },
});

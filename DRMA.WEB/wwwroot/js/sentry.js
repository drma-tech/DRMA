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
    /wasm simd/i
];

const version = appVersion?.trim() ? appVersion : "error";

Sentry.init({
    dsn: servicesConfig.SentryDsn,
    release: `drma-js@${version}`,
    environment: env,
    beforeSend(event) {
        const message = event.exception?.values?.[0]?.value || event.message || "";

        if (message && ignoredErrors.some(err => err.test(message))) {
            return null;
        }

        event.tags = {
            "custom.version": version,
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

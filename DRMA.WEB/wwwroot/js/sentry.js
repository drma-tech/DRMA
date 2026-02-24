import { appVersion, servicesConfig } from "./main.js";
import { storage, environment } from "./utils.js";

Sentry.init({
    dsn: servicesConfig.SentryDsn,
    beforeSend(event) {
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
import { isBot, isOldBrowser } from "./main.js";
import { notification } from "./utils.js";

//avoid google (and others) search console execute this
if (!isBot && !isOldBrowser) {
    if (navigator.serviceWorker?.register) {
        navigator.serviceWorker.register("service-worker.js").catch((err) => {
            notification.showError(
                `Failed to register the service worker, which may affect platform functionality. Details: ${err.message}`
            );
            notification.sendLog(err, "navigator.serviceWorker.register");
        });
    }
}

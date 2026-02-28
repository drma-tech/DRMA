import { isBot, isOldBrowser } from "./main.js";
import { notification } from "./utils.js";

//avoid google (and others) search console execute this
if (!isBot && !isOldBrowser) {
    if ('serviceWorker' in navigator) {
        navigator.serviceWorker.register("service-worker.js").catch((err) => {
            notification.showError(
                "Offline mode could not be activated (slow connection?). Try refreshing the page or closing and reopening the app."
            );
        });
    }
}

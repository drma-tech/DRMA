import { isBot, disableServiceWorker } from "./main.js";
import { notification } from "./utils.js";

//avoid google (and others) search console or possible bots
if (!isBot && !disableServiceWorker) {
    if ('serviceWorker' in navigator) {
        navigator.serviceWorker.register("service-worker.js", { updateViaCache: 'none' }).catch((err) => {
            notification.showError(
                "Offline mode could not be activated (slow connection?). Try refreshing the page or closing and reopening the app."
            );
        });
    }
}
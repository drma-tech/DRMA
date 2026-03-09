window.browser = window.bowser.getParser(window.navigator.userAgent);

export const isBot =
    /google|baidu|bingbot|duckduckbot|teoma|slurp|yandex|toutiao|bytespider|applebot/i.test(
        navigator.userAgent
    );

// supports WebAssembly SIMD
export const isOldBrowser = window.browser.satisfies({
    chrome: "<91",
    edge: "<91",
    safari: "<16.4",
});
// validate only if it's a webapp
export const isBotBrowser = window.browser.satisfies({
    chrome: "<134", //feb 2025
});
export const isLocalhost = location.host.includes("localhost");
export const isDev = location.hostname.includes("dev.");
export const isWebview = /webtonative/i.test(navigator.userAgent);
export const isPrintScreen = location.href.includes("printscreen");

export const servicesConfig = {
    AnalyticsCode: "G-41Q6D56912",
    ClarityKey: "r5ie3u2oos",
    UserBackToken: "A-A2J4M5NKCbDp1QyQe7ogemmmq",
    SentryDsn: "https://9d67a029559a76d507d4437b586e778f@o4510938040041472.ingest.us.sentry.io/4510942795005952",
};

export const baseApiUrl = isLocalhost ? "http://localhost:7164" : "";

// Disable robots for dev environment
if (isDev) {
    const meta = document.createElement("meta");
    meta.name = "robots";
    meta.content = "noindex, nofollow";
    document.head.appendChild(meta);
}

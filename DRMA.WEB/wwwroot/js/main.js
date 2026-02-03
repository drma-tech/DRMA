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
export const isLocalhost = location.host.includes("localhost");
export const isDev = location.hostname.includes("dev.");
export const isWebview = /webtonative/i.test(navigator.userAgent);
export const isPrintScreen = location.href.includes("printscreen");
export const appVersion = (
    await fetch("/build-date.txt")
        .then((r) => r.text())
        .catch(() => "version-error")
).trim();

export const servicesConfig = {
    AnalyticsCode: "G-41Q6D56912",
    ClarityKey: "r5ie3u2oos",
    UserBackToken: "A-A2J4M5NKCbDp1QyQe7ogemmmq",
    UserBackSurveyKey: "",
};

export const baseApiUrl = isLocalhost ? "http://localhost:7164" : "";

// Disable robots for dev environment
if (isDev) {
    const meta = document.createElement("meta");
    meta.name = "robots";
    meta.content = "noindex, nofollow";
    document.head.appendChild(meta);
}

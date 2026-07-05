"use strict";

const ua = navigator.userAgent;
window.browser = window.bowser?.getParser ? window.bowser.getParser(ua) : null;
const botUAs = ["google", "baidu", "bingbot", "duckduckbot", "teoma", "slurp", "yandex", "toutiao", "bytespider", "applebot", "crawler"];
const isBot = botUAs.some(bot => ua.toLowerCase().includes(bot)) || navigator.webdriver;

function testBrowserVersion(rules, ignore = false, fallback = false) {
    if (ignore) return false;

    if (!window.browser) return fallback;

    try {
        return window.browser.satisfies(rules);
    } catch {
        return fallback;
    }
}

const wasmSupported = typeof WebAssembly === "object";
const isLocalhost = window.location.hostname === "localhost";
const isPrerendering = window.location.hostname === "127.0.0.1"
const isDev = location.hostname.includes("develop");
const isWebview = /webtonative/i.test(ua);
const isPrintScreen = location.href.includes("printscreen");

function supportsWasmSimd() {
    if (typeof WebAssembly === "undefined") return false;
    const bytes = new Uint8Array([
        0x00, 0x61, 0x73, 0x6d, 0x01, 0x00, 0x00, 0x00,
        0x01, 0x05, 0x01, 0x60, 0x00, 0x01, 0x7b,
        0x03, 0x02, 0x01, 0x00,
        0x0a, 0x0a, 0x01, 0x08, 0x00, 0x41, 0x00, 0xfd, 0x0f, 0xfd, 0x62, 0x0b
    ]);
    return WebAssembly.validate(bytes);
}

//The browser does not support WASM or SIMD.
const blazorSupported = wasmSupported && supportsWasmSimd();

//probably a bot, so doesnt support sw
const disableServiceWorker = testBrowserVersion(
    {
        chrome: "<134", //special case (usually bots)
        edge: "<91", //may 21
        firefox: "<89", //may 21
        safari: "<16.4", //mar 23
        opera: "<77", //jun 21
    },
    isWebview,
    true // uncertain environment → disable
);

const servicesConfig = {
    AnalyticsCode: "G-41Q6D56912",
    ClarityKey: "r5ie3u2oos",
    UserBackToken: "A-A2J4M5NKCbDp1QyQe7ogemmmq",
    SentryDsn: "https://9d67a029559a76d507d4437b586e778f@o4510938040041472.ingest.us.sentry.io/4510942795005952",
};

const baseApiUrl = isLocalhost ? "http://localhost:7164" : "";

window.appConfig = {
    isBot,
    blazorSupported,
    disableServiceWorker,
    isLocalhost,
    isPrerendering,
    isDev,
    isWebview,
    isPrintScreen,
    servicesConfig,
    baseApiUrl
};

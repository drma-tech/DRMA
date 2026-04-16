const ua = navigator.userAgent;

window.browser = window?.bowser?.getParser
    ? window.bowser.getParser(ua)
    : null;

const botUAs = ["google", "baidu", "bingbot", "duckduckbot", "teoma", "slurp", "yandex", "toutiao", "bytespider", "applebot", "crawler"];

export const isBot = botUAs.some(bot => ua.toLowerCase().includes(bot)) || navigator.webdriver;

function testBrowserVersion(rules, ignore = false, fallback = false) {
    if (ignore) return false;

    if (!window.browser) return fallback;

    try {
        return window.browser.satisfies(rules);
    } catch {
        return fallback;
    }
}

//browser versions not compatible with SIMD (Some versions above for stability)
export const hideBlazorIndex = testBrowserVersion(
    {
        chrome: "<96", //nov 21
        edge: "<96", //nov 21
        firefox: "<96", //jan 22
        safari: "<16.6", //jul 23
        opera: "<82", //dec 21
    },
    /Mediapartners-Google/i.test(ua),
    false // uncertain environment → allow
);

//probably a bot, so doesnt support sw
export const disableServiceWorker = testBrowserVersion(
    {
        chrome: "<134", //special case (usually bots)
        edge: "<96", //nov 21
        firefox: "<96", //jan 22
        safari: "<16.6", //jul 23
        opera: "<82", //dec 21
    },
    false,
    true // uncertain environment → disable
);

export const isLocalhost = location.host.includes("localhost");
export const isDev = location.hostname.includes("develop");
export const isWebview = /webtonative/i.test(ua);
export const isPrintScreen = location.href.includes("printscreen");

export const servicesConfig = {
    AnalyticsCode: "G-41Q6D56912",
    ClarityKey: "r5ie3u2oos",
    UserBackToken: "A-A2J4M5NKCbDp1QyQe7ogemmmq",
    SentryDsn: "https://9d67a029559a76d507d4437b586e778f@o4510938040041472.ingest.us.sentry.io/4510942795005952",
};

export const baseApiUrl = isLocalhost ? "http://localhost:7164" : "";

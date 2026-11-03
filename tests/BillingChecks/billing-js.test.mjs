import { test } from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import vm from 'node:vm';

const source = readFileSync(new URL('../../HeartJourneyWeb/wwwroot/js/billing.js', import.meta.url), 'utf8');
function setup({ play = false, marker = false, fail = false, cancel = false } = {}) {
    const events = [];
    const storage = new Map();
    let resolvePurchase;
    const purchase = new Promise(resolve => { resolvePurchase = resolve; });
    const context = {
        URL, Intl, WeakMap, location: { href: 'https://app.test/' + (marker ? '?billing=google_play' : '') },
        document: { referrer: '' }, navigator: { language: 'en-US' },
        sessionStorage: { getItem: k => storage.get(k), setItem: (k, v) => storage.set(k, v) },
        PaymentRequest: class {
            constructor(methods) { events.push(methods[0]); }
            show() { events.push('show'); return cancel ? Promise.reject(Object.assign(new Error(), { name: 'AbortError' })) : purchase; }
        }
    };
    context.window = context;
    if (play) context.getDigitalGoodsService = async () => {
        if (fail) throw new Error('Unavailable');
        return {
            getDetails: async ids => [{ itemId: ids[0], price: { currency: 'USD', value: '9.99' } }],
            listPurchases: async () => [{ itemId: 'journey_unlock', purchaseToken: 'server-only-token' }]
        };
    };
    vm.runInNewContext(source, context);
    return { billing: context.journeyBilling, events, resolvePurchase, context };
}

test('regular browser uses Stripe, including API rejection without Play context', async () => {
    assert.equal(await setup().billing.detectProvider(), 'stripe');
    assert.equal(await setup({ play: true, fail: true }).billing.detectProvider(), 'stripe');
});
test('known Play launch fails closed and survives local navigation', async () => {
    const { billing, context } = setup({ marker: true });
    assert.equal(await billing.detectProvider(), 'unavailable');
    context.location.href = 'https://app.test/auth';
    assert.equal(await billing.detectProvider(), 'unavailable');
});
test('available Play billing returns Google Play and store product price', async () => {
    const { billing } = setup({ play: true });
    assert.equal(await billing.detectProvider(), 'google_play');
    assert.equal((await billing.prepare('journey_unlock')).price, '$9.99');
    assert.equal((await billing.listPurchases())[0].itemId, 'journey_unlock');
});
test('payment opens synchronously, rerenders do not duplicate listeners, and server confirms access', async () => {
    const { billing, events, resolvePurchase } = setup({ play: true });
    await billing.detectProvider();
    await billing.prepare('journey_unlock');
    const listeners = new Set();
    const button = { disabled: false, addEventListener: (_, f) => listeners.add(f), removeEventListener: (_, f) => listeners.delete(f) };
    const callback = { invokeMethodAsync: async (method, ...args) => {
        events.push(method);
        if (method === 'OnPlayPurchaseCompleted') {
            assert.deepEqual(args, ['journey_unlock', 'purchase-token']);
            return 'granted';
        }
    } };
    billing.bind(button, callback);
    billing.bind(button, callback);
    assert.equal(listeners.size, 1);
    const pending = [...listeners][0]({ preventDefault() {}, stopImmediatePropagation() {} });
    assert.equal(events[1], 'show');
    assert.equal(events[2], 'OnPlayPurchaseStarted');
    assert.equal(button.disabled, true);
    resolvePurchase({ details: { purchaseToken: 'purchase-token' }, complete: async state => events.push(state) });
    await pending;
    assert.ok(events.includes('success'));
    assert.equal(button.disabled, false);
    billing.unbind(button);
    assert.equal(listeners.size, 0);
});
test('canceling a Play purchase never invokes verification', async () => {
    const { billing, events } = setup({ play: true, cancel: true });
    await billing.detectProvider();
    await billing.prepare('journey_unlock');
    let listener;
    billing.bind({ disabled: false, addEventListener: (_, f) => { listener = f; } }, {
        invokeMethodAsync: async (method, canceled) => events.push([method, canceled])
    });
    await listener({ preventDefault() {}, stopImmediatePropagation() {} });
    assert.ok(events.some(e => Array.isArray(e) && e[0] === 'OnPlayPurchaseFailed' && e[1] === true));
    assert.ok(!events.some(e => Array.isArray(e) && e[0] === 'OnPlayPurchaseCompleted'));
});

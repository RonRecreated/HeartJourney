import { test, beforeEach } from 'node:test';
import assert from 'node:assert/strict';

const key = await crypto.subtle.generateKey({ name: 'RSASSA-PKCS1-v1_5', modulusLength: 2048,
    publicExponent: new Uint8Array([1, 0, 1]), hash: 'SHA-256' }, true, ['sign', 'verify']);
const pem = '-----BEGIN PRIVATE KEY-----\n' + Buffer.from(await crypto.subtle.exportKey('pkcs8', key.privateKey)).toString('base64')
    + '\n-----END PRIVATE KEY-----';
const env = {
    SUPABASE_URL: 'https://db.test', SUPABASE_SERVICE_ROLE_KEY: 'admin-key', SUPABASE_ANON_KEY: 'anon-key',
    GOOGLE_PLAY_PACKAGE_NAME: 'com.example.journey', BILLING_ALLOWED_ORIGINS: 'https://app.test',
    BILLING_SYNC_SECRET: 'private-cron-secret-with-at-least-32-characters',
    GOOGLE_PLAY_SERVICE_ACCOUNT_JSON: JSON.stringify({ client_email: 'billing@example.test', private_key: pem })
};
let handler;
globalThis.Deno = { env: { get: name => env[name] }, serve: callback => { handler = callback; } };
await import('../../supabase/functions/verify-google-play-purchase/index.ts');
const ledger = new Map();
const google = new Map();
let ackCalls;
const json = (value, status = 200) => new Response(JSON.stringify(value), { status });

beforeEach(() => {
    ledger.clear(); google.clear(); ackCalls = 0;
    google.set('valid-token', {
        purchaseStateContext: { purchaseState: 'PURCHASED' }, productLineItem: [{ productId: 'journey_unlock' }],
        acknowledgementState: 'ACKNOWLEDGEMENT_STATE_PENDING'
    });
    globalThis.fetch = async (url, init = {}) => {
        url = String(url);
        if (url.endsWith('/auth/v1/user')) {
            const token = init.headers.Authorization;
            return token === 'Bearer bad' ? json({}, 401)
                : json({ id: token === 'Bearer second' ? 'user-two' : 'user-one', role: 'authenticated' });
        }
        if (url.startsWith('https://db.test/rest/v1/google_play_products?'))
            return json([{ product_id: 'journey_unlock', journey_slug: 'relationship', is_active: true }]);
        if (url.endsWith('/rpc/record_google_play_purchase')) {
            const record = JSON.parse(init.body);
            const existing = ledger.get(record.p_token);
            if (existing && existing.p_user !== record.p_user) return json({}, 409);
            ledger.set(record.p_token, record);
            return json('purchase-id');
        }
        if (url.startsWith('https://db.test/rest/v1/google_play_purchases?'))
            return json([...ledger.values()].map(p => ({ id: 'purchase-id', purchase_token: p.p_token,
                user_id: p.p_user, product_id: p.p_product, journey_slug: p.p_journey, package_name: p.p_package })));
        if (url === 'https://oauth2.googleapis.com/token') {
            assert.match(init.body.get('assertion'), /^[^.]+\.[^.]+\.[^.]+$/);
            return json({ access_token: 'google-access' });
        }
        if (url.startsWith('https://androidpublisher.googleapis.com/')) {
            assert.equal(init.headers.Authorization, 'Bearer google-access');
            if (url.endsWith(':acknowledge')) {
                ackCalls++;
                assert.ok(ledger.has('valid-token'), 'ownership saved before acknowledging');
                google.get('valid-token').acknowledgementState = 'ACKNOWLEDGEMENT_STATE_ACKNOWLEDGED';
                return new Response(null, { status: 204 });
            }
            const token = decodeURIComponent(url.split('/').at(-1));
            return google.has(token) ? json(google.get(token)) : json({}, 404);
        }
        throw new Error('Unexpected URL: ' + url);
    };
});

function request(body, { token = 'user', origin = 'https://app.test', secret } = {}) {
    const headers = { Authorization: 'Bearer ' + token, 'Content-Type': 'application/json', Origin: origin };
    if (secret) headers['x-billing-sync-secret'] = secret;
    return handler(new Request('https://db.test/functions/v1/verify-google-play-purchase', {
        method: 'POST', headers, body: JSON.stringify(body)
    }));
}
const verification = { action: 'verify', journeySlug: 'relationship', productId: 'journey_unlock', purchaseToken: 'valid-token' };

test('untrusted origins and invalid JWTs cannot reach purchase verification', async () => {
    assert.equal((await request(verification, { origin: 'https://attacker.test' })).status, 403);
    assert.equal((await request(verification, { token: 'bad' })).status, 401);
    assert.equal(ledger.size, 0);
});
test('catalog comes from server mapping and contains no credentials', async () => {
    const response = await request({ action: 'catalog', journeySlug: 'relationship' });
    assert.deepEqual(await response.json(), { productId: 'journey_unlock' });
});
test('verified purchase is acknowledged and linked to authenticated user, ignoring supplied user ID', async () => {
    const response = await request({ ...verification, userId: 'attacker-selected-user' });
    assert.deepEqual(await response.json(), { status: 'granted' });
    assert.equal(ledger.get('valid-token').p_user, 'user-one');
    assert.equal(ledger.get('valid-token').p_acknowledged, true);
    assert.equal(ackCalls, 1);
    assert.deepEqual(await (await request(verification)).json(), { status: 'granted' });
    assert.equal(ackCalls, 1, 'retries do not acknowledge twice');
});
test('purchase tokens cannot be reassigned to another app account', async () => {
    await request(verification);
    assert.equal((await request(verification, { token: 'second' })).status, 409);
    assert.equal(ledger.get('valid-token').p_user, 'user-one');
});
test('pending payments reserve ownership but are not acknowledged or granted', async () => {
    google.get('valid-token').purchaseStateContext.purchaseState = 'PENDING';
    const response = await request(verification);
    assert.deepEqual(await response.json(), { status: 'pending' });
    assert.equal(ledger.get('valid-token').p_state, 'PENDING');
    assert.equal(ledger.get('valid-token').p_acknowledged, false);
    assert.equal(ackCalls, 0);
});
test('Google product mismatch and nonexistent tokens never create entitlement records', async () => {
    google.get('valid-token').productLineItem[0].productId = 'different_product';
    assert.equal((await request(verification)).status, 503);
    assert.equal((await request({ ...verification, purchaseToken: 'missing-token' })).status, 503);
    assert.equal(ledger.size, 0);
});
test('reconciliation requires its own secret and persists Google cancellation', async () => {
    await request(verification);
    google.get('valid-token').purchaseStateContext.purchaseState = 'CANCELLED';
    assert.equal((await request({ action: 'reconcile' })).status, 401);
    const response = await request({ action: 'reconcile' }, { secret: env.BILLING_SYNC_SECRET });
    assert.deepEqual(await response.json(), { checked: 1, failed: 0 });
    assert.equal(ledger.get('valid-token').p_state, 'CANCELLED');
});

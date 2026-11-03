import { googleAccessToken, readPurchase, acknowledgePurchase, validatePurchase } from './google-play.ts';

function required(name: string) {
    const value = Deno.env.get(name);
    if (!value) throw new Error(`Missing ${name}`);
    return value;
}

class RequestError extends Error {
    status: number;
    constructor(status: number, message: string) { super(message); this.status = status; }
}

// No third-party dependencies. All sensitive API calls run on this server.
Deno.serve(async (req: Request) => {
    const allowed = (Deno.env.get('BILLING_ALLOWED_ORIGINS') ?? '').split(',').map(x => x.trim());
    const origin = req.headers.get('origin');
    const headers: Record<string, string> = {
        'Content-Type': 'application/json', 'Cache-Control': 'no-store', Vary: 'Origin',
        'Access-Control-Allow-Headers': 'authorization, apikey, content-type, x-billing-sync-secret',
        'Access-Control-Allow-Methods': 'POST, OPTIONS'
    };
    if (origin && allowed.includes(origin)) headers['Access-Control-Allow-Origin'] = origin;
    const reply = (status: number, body: unknown) => new Response(JSON.stringify(body), { status, headers });
    if (origin && !allowed.includes(origin)) return reply(403, { error: 'Origin not allowed' });
    if (req.method === 'OPTIONS') return new Response(null, { status: 204, headers });
    if (req.method !== 'POST') return reply(405, { error: 'POST required' });

    try {
        const text = await req.text();
        if (text.length > 16000) throw new RequestError(413, 'Request too large');
        let body;
        try { body = JSON.parse(text); } catch { throw new RequestError(400, 'Invalid JSON'); }
        if (!body || typeof body !== 'object') throw new RequestError(400, 'Invalid request');
        const supabase = required('SUPABASE_URL');
        const adminKey = required('SUPABASE_SERVICE_ROLE_KEY');
        const packageName = required('GOOGLE_PLAY_PACKAGE_NAME');
        const admin = async (path: string, init: RequestInit = {}) => {
            const response = await fetch(supabase + '/rest/v1/' + path, {
                ...init, headers: { apikey: adminKey, Authorization: `Bearer ${adminKey}`,
                    'Content-Type': 'application/json', ...init.headers }, signal: AbortSignal.timeout(15000)
            });
            if (!response.ok) {
                if (response.status === 409) throw new RequestError(409, 'Purchase ownership conflict');
                throw new Error(`Billing database request failed (${response.status})`);
            }
            return response.status === 204 ? null : response.json();
        };
        const record = async (token: string, userId: string, productId: string, journeySlug: string,
            state: string, acknowledged: boolean, orderId?: string) => admin('rpc/record_google_play_purchase', {
                method: 'POST', body: JSON.stringify({ p_token: token, p_user: userId, p_product: productId,
                    p_journey: journeySlug, p_package: packageName, p_state: state,
                    p_acknowledged: acknowledged, p_order: orderId ?? null })
            });
        let googleToken: string | undefined;
        const verify = async (token: string, userId: string, productId: string, journeySlug: string) => {
            googleToken ??= await googleAccessToken(required('GOOGLE_PLAY_SERVICE_ACCOUNT_JSON'));
            const purchase = await readPurchase(googleToken, packageName, token);
            const result = validatePurchase(purchase, productId, Deno.env.get('GOOGLE_PLAY_ALLOW_TEST_PURCHASES') === 'true');
            // Save ownership durably before acknowledgement; retries cannot transfer it.
            await record(token, userId, productId, journeySlug, result.state, result.acknowledged, purchase.orderId);
            if (result.state === 'PURCHASED' && !result.acknowledged) {
                try { await acknowledgePurchase(googleToken, packageName, productId, token); }
                catch {
                    // Another retry may have acknowledged the same purchase concurrently.
                    const latest = validatePurchase(await readPurchase(googleToken, packageName, token), productId,
                        Deno.env.get('GOOGLE_PLAY_ALLOW_TEST_PURCHASES') === 'true');
                    if (latest.state !== 'PURCHASED' || !latest.acknowledged) throw new Error('Acknowledgement not confirmed');
                }
                await record(token, userId, productId, journeySlug, result.state, true, purchase.orderId);
            }
            return result.state === 'PURCHASED' ? 'granted' : result.state === 'PENDING' ? 'pending' : 'canceled';
        };

        if (body.action === 'reconcile') {
            const secret = required('BILLING_SYNC_SECRET');
            const supplied = req.headers.get('x-billing-sync-secret') ?? '';
            const digest = async (value: string) => new Uint8Array(await crypto.subtle.digest('SHA-256', new TextEncoder().encode(value)));
            const [a, b] = await Promise.all([digest(secret), digest(supplied)]);
            let difference = 0;
            for (let i = 0; i < a.length; i++) difference |= a[i] ^ b[i];
            if (secret.length < 32 || difference !== 0) throw new RequestError(401, 'Unauthorized');
            // Oldest first gives successive runs coverage without exposing tokens to cron.
            const rows = await admin('google_play_purchases?select=*&purchase_state=neq.CANCELLED&order=last_checked_at.asc&limit=50');
            let checked = 0, failed = 0;
            for (const row of rows) {
                try {
                    if (row.package_name !== packageName) throw new Error('Package mismatch');
                    await verify(row.purchase_token, row.user_id, row.product_id, row.journey_slug);
                    checked++;
                } catch {
                    failed++;
                    console.error('Billing reconciliation failed for record', row.id);
                    // Keep last_verified_at accurate while preventing a bad record
                    // from starving other purchases on every scheduled run.
                    await admin('google_play_purchases?id=eq.' + encodeURIComponent(row.id), {
                        method: 'PATCH', body: JSON.stringify({ last_checked_at: new Date().toISOString() })
                    });
                }
            }
            return reply(200, { checked, failed });
        }

        // verify_jwt is disabled for the cron action. Every user action still verifies
        // the JWT against Supabase Auth; the caller cannot choose their user ID.
        const authorization = req.headers.get('authorization') ?? '';
        if (!authorization.startsWith('Bearer ')) throw new RequestError(401, 'Sign in required');
        const authResponse = await fetch(supabase + '/auth/v1/user', {
            headers: { apikey: required('SUPABASE_ANON_KEY'), Authorization: authorization },
            signal: AbortSignal.timeout(15000)
        });
        if (!authResponse.ok) throw new RequestError(401, 'Invalid session');
        const user = await authResponse.json();
        if (!user.id || user.role !== 'authenticated') throw new RequestError(401, 'Sign in required');
        if (typeof body.journeySlug !== 'string' || body.journeySlug.length > 200)
            throw new RequestError(400, 'Journey required');
        const products = await admin('google_play_products?select=product_id,journey_slug,is_active&journey_slug=eq.' + encodeURIComponent(body.journeySlug));
        const product = products[0];
        if (!product) throw new RequestError(400, 'Journey product not configured');
        if (body.action === 'catalog') {
            if (!product.is_active) throw new RequestError(400, 'Product unavailable');
            return reply(200, { productId: product.product_id });
        }
        if (body.action !== 'verify' || body.productId !== product.product_id
            || typeof body.purchaseToken !== 'string' || body.purchaseToken.length < 1 || body.purchaseToken.length > 8192)
            throw new RequestError(400, 'Invalid purchase');
        const status = await verify(body.purchaseToken, user.id, product.product_id, product.journey_slug);
        return reply(200, { status });
    } catch (error) {
        // Never log request bodies, purchase tokens, private keys, or Google's responses.
        if (error instanceof RequestError) return reply(error.status, { error: error.message });
        console.error('Google Play billing request failed');
        return reply(503, { error: 'Purchase verification unavailable. Please retry restoration.' });
    }
});

export type GooglePurchase = {
    purchaseStateContext?: { purchaseState?: string };
    productLineItem?: { productId?: string; productOfferDetails?: {
        consumptionState?: string; rentOfferDetails?: unknown; quantity?: number
    } }[];
    acknowledgementState?: string;
    testPurchaseContext?: unknown;
    orderId?: string;
};

export function validatePurchase(purchase: GooglePurchase, productId: string, allowTests: boolean) {
    const state = purchase.purchaseStateContext?.purchaseState;
    if (!['PURCHASED', 'PENDING', 'CANCELLED'].includes(state ?? ''))
        throw new Error('Unrecognized purchase state');
    const items = purchase.productLineItem;
    if (!items || items.length !== 1 || items[0].productId !== productId)
        throw new Error('Product does not match purchase');
    if (items[0].productOfferDetails?.consumptionState === 'CONSUMPTION_STATE_CONSUMED')
        throw new Error('A permanent journey unlock must not be consumed');
    if (items[0].productOfferDetails?.rentOfferDetails || (items[0].productOfferDetails?.quantity ?? 1) !== 1)
        throw new Error('Expected one permanent journey unlock');
    if (purchase.testPurchaseContext && !allowTests)
        throw new Error('Test purchases are disabled');
    return { state: state!, acknowledged: purchase.acknowledgementState === 'ACKNOWLEDGEMENT_STATE_ACKNOWLEDGED' };
}

function base64url(bytes: Uint8Array) {
    return btoa(String.fromCharCode(...bytes)).replace(/=/g, '').replace(/\+/g, '-').replace(/\//g, '_');
}

export async function googleAccessToken(accountJson: string): Promise<string> {
    const account = JSON.parse(accountJson);
    const encoder = new TextEncoder();
    const encode = (value: unknown) => base64url(encoder.encode(JSON.stringify(value)));
    const now = Math.floor(Date.now() / 1000);
    const unsigned = encode({ alg: 'RS256', typ: 'JWT' }) + '.' + encode({
        iss: account.client_email, scope: 'https://www.googleapis.com/auth/androidpublisher',
        aud: 'https://oauth2.googleapis.com/token', iat: now, exp: now + 3600
    });
    const pem = account.private_key.replace(/-----[^-]+-----/g, '').replace(/\s/g, '');
    const key = await crypto.subtle.importKey('pkcs8', Uint8Array.from(atob(pem), c => c.charCodeAt(0)),
        { name: 'RSASSA-PKCS1-v1_5', hash: 'SHA-256' }, false, ['sign']);
    const signature = await crypto.subtle.sign('RSASSA-PKCS1-v1_5', key, encoder.encode(unsigned));
    const response = await fetch('https://oauth2.googleapis.com/token', {
        method: 'POST', signal: AbortSignal.timeout(15000),
        body: new URLSearchParams({ grant_type: 'urn:ietf:params:oauth:grant-type:jwt-bearer',
            assertion: unsigned + '.' + base64url(new Uint8Array(signature)) })
    });
    if (!response.ok) throw new Error('Google authentication failed');
    const result = await response.json();
    if (!result.access_token) throw new Error('Google access token missing');
    return result.access_token;
}

export async function readPurchase(accessToken: string, packageName: string, token: string): Promise<GooglePurchase> {
    const response = await fetch(`https://androidpublisher.googleapis.com/androidpublisher/v3/applications/${encodeURIComponent(packageName)}/purchases/productsv2/tokens/${encodeURIComponent(token)}`, {
        headers: { Authorization: `Bearer ${accessToken}` }, signal: AbortSignal.timeout(15000)
    });
    // A failed lookup must never create or revoke access. Retry it on the next run.
    if (!response.ok) throw new Error(`Google purchase lookup failed (${response.status})`);
    return response.json();
}

export async function acknowledgePurchase(accessToken: string, packageName: string, productId: string, token: string) {
    const response = await fetch(`https://androidpublisher.googleapis.com/androidpublisher/v3/applications/${encodeURIComponent(packageName)}/purchases/products/${encodeURIComponent(productId)}/tokens/${encodeURIComponent(token)}:acknowledge`, {
        method: 'POST', headers: { Authorization: `Bearer ${accessToken}`, 'Content-Type': 'application/json' },
        body: '{}', signal: AbortSignal.timeout(15000)
    });
    if (!response.ok) throw new Error(`Google acknowledgement failed (${response.status})`);
}

// Routing hints never grant access. Purchase tokens are verified only on the server.
window.journeyBilling = (() => {
    const method = 'https://play.google.com/billing';
    let service;
    let product;
    const bindings = new WeakMap();

    function isPlayLaunch() {
        const marker = new URL(location.href).searchParams.get('billing') === 'google_play'
            || document.referrer.startsWith('android-app://');
        try {
            if (marker) sessionStorage.setItem('journey.playLaunch', '1');
            return marker || sessionStorage.getItem('journey.playLaunch') === '1';
        } catch { return marker; }
    }

    async function detectProvider() {
        const playLaunch = isPlayLaunch();
        if (typeof window.getDigitalGoodsService === 'function') {
            try {
                service = await window.getDigitalGoodsService(method);
                if (service && typeof window.PaymentRequest === 'function') return 'google_play';
            } catch { /* A regular browser can expose the API without store support. */ }
        }
        return playLaunch ? 'unavailable' : 'stripe';
    }

    async function prepare(productId) {
        product = null;
        if (!service) throw new Error('Google Play billing unavailable.');
        const details = await service.getDetails([productId]);
        const item = details.find(x => x.itemId === productId);
        if (!item?.price) throw new Error('Google Play product unavailable.');
        product = { productId, price: new Intl.NumberFormat(navigator.language,
            { style: 'currency', currency: item.price.currency }).format(item.price.value) };
        return product;
    }

    function unbind(button) {
        const binding = bindings.get(button);
        if (binding) button.removeEventListener('click', binding.listener, true);
        bindings.delete(button);
    }

    function bind(button, callback) {
        const selected = product;
        if (!selected) throw new Error('Prepare a product before binding purchase.');
        if (bindings.get(button)?.productId === selected.productId) return;
        unbind(button);
        let busy = false;
        const listener = async event => {
            event.preventDefault();
            event.stopImmediatePropagation();
            if (busy || button.disabled) return;
            busy = true;
            button.disabled = true;
            let response;
            try {
                // No await, network request, or Blazor round trip before show().
                const request = new PaymentRequest([{ supportedMethods: method,
                    data: { sku: selected.productId } }], {
                    total: { label: 'Journey unlock', amount: { currency: 'USD', value: '0' } }
                });
                // Attach rejection handling immediately, including when Blazor is busy.
                const showing = request.show().then(value => ({ value }), error => ({ error }));
                await callback.invokeMethodAsync('OnPlayPurchaseStarted');
                const outcome = await showing;
                if (outcome.error) throw outcome.error;
                response = outcome.value;
                const token = response.details?.purchaseToken;
                if (!token) throw new Error('Missing purchase token.');
                const status = await callback.invokeMethodAsync('OnPlayPurchaseCompleted', selected.productId, token);
                try { await response.complete(status === 'granted' ? 'success' : 'fail'); } catch { }
            } catch (error) {
                if (response) { try { await response.complete('unknown'); } catch { } }
                try { await callback.invokeMethodAsync('OnPlayPurchaseFailed', error?.name === 'AbortError'); } catch { }
            } finally {
                busy = false;
                button.disabled = false;
            }
        };
        bindings.set(button, { listener, productId: selected.productId });
        button.addEventListener('click', listener, true);
    }

    async function listPurchases() {
        if (!service) throw new Error('Google Play billing unavailable.');
        return (await service.listPurchases()).map(p => ({ itemId: p.itemId, purchaseToken: p.purchaseToken }));
    }
    return { detectProvider, prepare, bind, unbind, listPurchases };
})();

// Routing hints never grant access. Purchase tokens are verified only on the server.
window.journeyBilling = (() => {
    const method = 'https://play.google.com/billing';
    let service;
    let product;
    const bindings = new WeakMap();
   
    let recoveryCallback = null;
    let recoveryBusy = false;
    let lastRecovery = 0;

    async function recoverOnResume() {
        if (!recoveryCallback || recoveryBusy || !service) return;
        if (document.visibilityState !== "visible") return;

        // Avoid repeated requests from multiple focus events.
        if (Date.now() - lastRecovery < 10000) return;
        lastRecovery = Date.now();
        recoveryBusy = true;

        try {
            const purchases = await listPurchases();
            const owned = purchases.filter(
                p => p.itemId === "relationship_journey_unlock"
            );

            if (owned.length > 0) {
                await recoveryCallback.invokeMethodAsync(
                    "OnPlayRecoveryRequested"
                );
            }
        } catch (error) {
            console.warn(
                "[Journey Billing] Purchase recovery unavailable",
                error?.message
            );
        } finally {
            recoveryBusy = false;
        }
    }

    function watchPurchases(callback) {
        recoveryCallback = callback;
        document.addEventListener(
            "visibilitychange",
            recoverOnResume
        );
        window.addEventListener("pageshow", recoverOnResume);
        window.addEventListener("focus", recoverOnResume);
    }

    function stopWatchingPurchases() {
        recoveryCallback = null;
        document.removeEventListener(
            "visibilitychange",
            recoverOnResume
        );
        window.removeEventListener("pageshow", recoverOnResume);
        window.removeEventListener("focus", recoverOnResume);
    }

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

        console.info("[Journey Billing] Detection started", {
            playLaunch,
            digitalGoodsAPI:
                typeof window.getDigitalGoodsService,
            paymentRequestAPI:
                typeof window.PaymentRequest
        });

        service = null;

        if (typeof window.getDigitalGoodsService === "function") {
            try {
                service = await window.getDigitalGoodsService(method);

                if (
                    service &&
                    typeof window.PaymentRequest === "function"
                ) {
                    console.info(
                        "[Journey Billing] Google Play Billing available"
                    );
                    return "google_play";
                }

                console.warn(
                    "[Journey Billing] Billing service or PaymentRequest unavailable"
                );
            } catch (error) {
                console.warn(
                    "[Journey Billing] Digital Goods API failed",
                    error?.name,
                    error?.message
                );
            }
        }

        console.info(
            "[Journey Billing] Provider result:",
            playLaunch ? "unavailable" : "stripe"
        );

        return playLaunch ? "unavailable" : "stripe";
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
    return { detectProvider, prepare, bind, unbind, listPurchases, watchPurchases, stopWatchingPurchases };
})();

window.journeyRecreatedPwa = {
    dotNetReference: null,
    registration: null,
    refreshing: false,

    initialize: async function (dotNetReference) {
        if (!('serviceWorker' in navigator)) {
            return;
        }

        this.dotNetReference = dotNetReference;

        const registration =
            await navigator.serviceWorker.getRegistration();

        if (!registration) {
            return;
        }

        this.registration = registration;

        try {
            await registration.update();
            console.info('PWA update: checked for a newer service worker.');
        }
        catch (error) {
            console.warn('PWA update: update check failed.', error);
        }

        // An update may already be waiting when Blazor starts.
        if (registration.waiting) {
            await this.notifyUpdateAvailable();
        }

        registration.addEventListener('updatefound', () => {
            const worker = registration.installing;

            if (!worker) {
                return;
            }

            worker.addEventListener('statechange', async () => {
                if (worker.state === 'installed' &&
                    navigator.serviceWorker.controller) {

                    await this.notifyUpdateAvailable();
                }
            });
        });

        navigator.serviceWorker.addEventListener(
            'controllerchange',
            () => {
                if (this.refreshing) {
                    return;
                }

                this.refreshing = true;
                window.location.reload();
            });
    },

    notifyUpdateAvailable: async function () {
        if (!this.dotNetReference) {
            return;
        }

        await this.dotNetReference.invokeMethodAsync(
            'ShowUpdateAvailable');
    },

    applyUpdate: async function () {
        const registration =
            await navigator.serviceWorker.getRegistration();

        if (!registration) {
            console.warn('PWA update: no service worker registration found.');
            return;
        }

        const waitingWorker = registration.waiting;

        if (!waitingWorker) {
            console.warn('PWA update: no waiting service worker found.');
            return;
        }

        console.info('PWA update: activating waiting service worker.');

        let reloading = false;

        const reloadPage = () => {
            if (reloading) {
                return;
            }

            reloading = true;

            console.info('PWA update: reloading with new version.');

            window.location.reload();
        };

        // Reload when the waiting worker actually reaches activated.
        waitingWorker.addEventListener('statechange', () => {
            console.info(
                `PWA update: worker state changed to ${waitingWorker.state}.`
            );

            if (waitingWorker.state === 'activated') {
                reloadPage();
            }
        });

        // Keep controllerchange as another reliable signal.
        navigator.serviceWorker.addEventListener(
            'controllerchange',
            reloadPage,
            { once: true }
        );

        waitingWorker.postMessage({
            type: 'SKIP_WAITING'
        });

        // Fallback in case the browser activates the worker
        // without delivering controllerchange/statechange as expected.
        setTimeout(async () => {
            const latestRegistration =
                await navigator.serviceWorker.getRegistration();

            if (!latestRegistration?.waiting) {
                reloadPage();
            }
        }, 2000);
    }
};
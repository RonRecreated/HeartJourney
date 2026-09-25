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

    applyUpdate: function () {
        if (!this.registration?.waiting) {
            return;
        }

        this.registration.waiting.postMessage({
            type: 'SKIP_WAITING'
        });
    }
};
window.journeyTurnstile = {
    widgets: {},

    render: function (elementId, siteKey, dotNetReference) {
        const tryRender = () => {
            if (!window.turnstile) {
                setTimeout(tryRender, 100);
                return;
            }

            const element = document.getElementById(elementId);

            if (!element) {
                return;
            }

            // Prevent duplicate rendering.
            if (this.widgets[elementId] !== undefined) {
                return;
            }

            const widgetId = window.turnstile.render(
                element,
                {
                    sitekey: siteKey,
                    theme: "light",
                    size: "flexible",

                    callback: function (token) {
                        dotNetReference.invokeMethodAsync(
                            "OnTurnstileSuccess",
                            token
                        );
                    },

                    "expired-callback": function () {
                        dotNetReference.invokeMethodAsync(
                            "OnTurnstileExpired"
                        );
                    },

                    "error-callback": function () {
                        dotNetReference.invokeMethodAsync(
                            "OnTurnstileError"
                        );
                    }
                }
            );

            this.widgets[elementId] = widgetId;
        };

        tryRender();
    },

    reset: function (elementId) {
        const widgetId =
            this.widgets[elementId];

        if (
            window.turnstile &&
            widgetId !== undefined
        ) {
            window.turnstile.reset(widgetId);
        }
    },

    remove: function (elementId) {
        const widgetId =
            this.widgets[elementId];

        if (
            window.turnstile &&
            widgetId !== undefined
        ) {
            window.turnstile.remove(widgetId);
            delete this.widgets[elementId];
        }
    }
};
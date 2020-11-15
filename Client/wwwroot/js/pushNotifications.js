(function () {
    const applicationServerPublicKey = 'BHrK6ZB6KchqcQEl8-eKROQsxvQKKJNEO6hPl-L2aCslvzgH1uH_PccrBoalSJ2kp7uFQldAfnfnnnSEEh00rWU';

    window.blazorPushNotifications = {
        /**
        * async getSubscription()
        * @return BrowserSubscriptionResult
        */
        getSubscription: async () => {
            if ('PushManager' in window
                && 'Notification' in window
                && 'showNotification' in ServiceWorkerRegistration.prototype) {
                switch (Notification.permission) {
                    case 'denied':
                        return { message: 'Notifications are not permitted in this browser' };
                    case 'default':
                        // note: an existing subscription may exist, but notification permission removed..
                        // return false to give the user a chance to re-enable notifications in requestSubscription()
                        return { hasExistingSubscription: false };
                    default:
                        const worker = await navigator.serviceWorker.getRegistration();
                        const existingSubscription = await worker.pushManager.getSubscription();
                        return { hasExistingSubscription: !!existingSubscription };
                }
            }
            return { message: 'This browser does not support notifications' };
        },
        /**
        * async requestSubscription()
        * @returns BrowserSubscriptionResult
        */
        requestSubscription: async () => {
            const worker = await navigator.serviceWorker.getRegistration();
            const existingSubscription = await worker.pushManager.getSubscription();

            if (!existingSubscription) {
                const newSubscription = await subscribe(worker);

                if (newSubscription) {
                    return {
                        newSubscription: {
                            url: newSubscription.endpoint,
                            p256dh: arrayBufferToBase64(newSubscription.getKey('p256dh')),
                            auth: arrayBufferToBase64(newSubscription.getKey('auth'))
                        }
                    };
                }
                return { message: 'Notifications are not permitted in this browser' };
            } else if (Notification.permission === 'default') {
                // user may have denied and then un-denied notification permissions,
                // so give them a chance to re-permit
                await Notification.requestPermission();
            }
            return { hasExistingSubscription: true };
        }
    };

    async function subscribe(worker) {
        try {
            // I think this calls Notification.requestPermission() under the hood
            return await worker.pushManager.subscribe({
                userVisibleOnly: true,
                applicationServerKey: applicationServerPublicKey
            });
        } catch (error) {
            if (error.name === 'NotAllowedError') {
                return null;
            }
            throw error;
        }
    }

    function arrayBufferToBase64(buffer) {
        // https://stackoverflow.com/a/9458996
        var binary = '';
        var bytes = new Uint8Array(buffer);
        var len = bytes.byteLength;
        for (var i = 0; i < len; i++) {
            binary += String.fromCharCode(bytes[i]);
        }
        return window.btoa(binary);
    }
})();

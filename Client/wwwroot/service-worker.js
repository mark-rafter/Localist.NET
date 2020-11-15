// In development, always fetch from the network and do not enable offline support.
// This is because caching would make development more difficult (changes would not
// be reflected on the first load after each change).
self.addEventListener('fetch', () => { });
self.addEventListener('push', event => onPush(event));
self.addEventListener('notificationclick', event => onNotificationclick(event));

function onPush(event) {
    const payload = event.data.json();
    event.waitUntil(
        self.registration.showNotification('Localist', {
            body: payload.message,
            icon: 'ico/icon-512.png',
            silent: true,
            data: { url: payload.url }
        })
    );
}

function onNotificationclick(event) {
    event.notification.close();
    event.waitUntil(clients.openWindow(event.notification.data.url));
}

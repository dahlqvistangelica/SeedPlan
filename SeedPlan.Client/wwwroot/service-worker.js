// Try importing assets-manifest, but continue without it if missing
try {
    self.importScripts('./service-worker-assets.js');
} catch (e) {
    console.log('service-worker-assets.js saknas, kör utan cache-manifest');
}

self.addEventListener('install', event => event.waitUntil(onInstall(event)));
self.addEventListener('activate', event => event.waitUntil(onActivate(event)));

self.addEventListener('fetch', event => {
    if (event.request.method !== 'GET') return;

    const url = new URL(event.request.url);
    const allowedHosts = [
        'seedplan.up.railway.app',
        'seedplan.runasp.net'
    ];

    if (!allowedHosts.includes(url.hostname)) return;

    event.respondWith(onFetch(event));
});

const cacheNamePrefix = 'offline-cache-';
const cacheName = `${cacheNamePrefix}${self.assetsManifest?.version ?? 'v1'}`;

async function onInstall(event) {
    console.info('Service worker: Install');
    self.skipWaiting();
    if (!self.assetsManifest) return;

    const assetsRequests = self.assetsManifest.assets
        .map(asset => new Request(asset.url, { cache: 'no-cache' }));

    try {
        const cache = await caches.open(cacheName);
        await cache.addAll(assetsRequests);
    } catch (e) {
        console.log('Cache addAll misslyckades, försöker en och en:', e);
        const cache = await caches.open(cacheName);
        for (const request of assetsRequests) {
            try {
                await cache.add(request);
            } catch (err) {
                console.log('Kunde inte cacha:', request.url, err.message);
            }
        }
    }
}


async function onActivate(event) {
    console.info('Service worker: Activate');
    clients.claim();
    const cacheKeys = await caches.keys();
    await Promise.all(cacheKeys
        .filter(key => key.startsWith(cacheNamePrefix) && key !== cacheName)
        .map(key => caches.delete(key)));
}


async function onFetch(event) {
    const neverCache = [
        'session-cleanup.js',
        'appsettings.json',
        'notifications.js'
    ];

    const url = new URL(event.request.url);

    // Skip caching for navigation to specific pages
    if (event.request.mode === 'navigate') {
        return fetch(event.request);
    }

    if (neverCache.some(f => url.pathname.endsWith(f))) {
        return fetch(event.request);
    }

    if (!self.assetsManifest) return fetch(event.request);

    const shouldServeFromCache =
        self.assetsManifest.assets.some(asset => event.request.url.endsWith(asset.url));

    if (shouldServeFromCache) {
        const cache = await caches.open(cacheName);
        const cachedResponse = await cache.match(event.request);
        if (cachedResponse) return cachedResponse;
    }

    return fetch(event.request);
}
// PUSH NOTIFICATIONS
self.addEventListener('push', event => {
    console.log('Push mottaget', event);

    let data = { title: 'SeedPlan', body: 'Du har en påminnelse', url: '/sowings' };
    if (event.data) {
        try {
            data = event.data.json();
        } catch (e) {
            data.body = event.data.text();
        }
    }

    event.waitUntil(
        self.registration.showNotification(data.title, {
            body: data.body,
            icon: '/icon-512.png',
            badge: '/icon-512.png',
            vibrate: [200, 100, 200],
            data: { url: data.url || '/sowings' }
        })
    );
});


// Open app on clicked notification
self.addEventListener('notificationclick', event => {
    event.notification.close();
    event.waitUntil(
        clients.matchAll({ type: 'window' }).then(clientList => {
            for (const client of clientList) {
                if (client.url && 'focus' in client) {
                    return client.focus();
                }
            }
            if (clients.openWindow) {
                return clients.openWindow(event.notification.data?.url ?? '/');
            }
        })
    );
});
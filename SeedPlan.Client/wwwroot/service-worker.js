// Importera de filer som Blazor-bygget säger att vi behöver
self.importScripts('./service-worker-assets.js');

self.addEventListener('install', event => event.waitUntil(onInstall(event)));
self.addEventListener('activate', event => event.waitUntil(onActivate(event)));
self.addEventListener('fetch', event => event.respondWith(onFetch(event)));

const cacheNamePrefix = 'offline-cache-';
const cacheName = `${cacheNamePrefix}${self.assetsManifest.version}`;

async function onInstall(event) {
    console.info('Service worker: Install');
    // Cachea alla filer som behövs för att köra appen offline
    const assetsRequests = self.assetsManifest.assets
        .map(asset => new Request(asset.url, { integrity: asset.hash, cache: 'no-cache' }));
    const cache = await caches.open(cacheName);
    await cache.addAll(assetsRequests);
}

async function onActivate(event) {
    console.info('Service worker: Activate');
    // Rensa gamla cacher
    const cacheKeys = await caches.keys();
    await Promise.all(cacheKeys
        .filter(key => key.startsWith(cacheNamePrefix) && key !== cacheName)
        .map(key => caches.delete(key)));
}

async function onFetch(event) {
    let cachedResponse = null;
    if (event.request.method === 'GET') {
        // Försök hämta från cache först (snabbast)
        const shouldServeFromCache = event.request.mode === 'navigate' ||
            self.assetsManifest.assets.some(asset => event.request.url.endsWith(asset.url));

        if (shouldServeFromCache) {
            const cache = await caches.open(cacheName);
            cachedResponse = await cache.match(event.request);
        }
    }
    return cachedResponse || fetch(event.request);
}
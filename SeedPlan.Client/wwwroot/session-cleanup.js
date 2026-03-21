// Avregistrera service worker på localhost (utveckling)
if (window.location.hostname === 'localhost') {
    navigator.serviceWorker.getRegistrations().then(function (regs) {
        regs.forEach(function (reg) {
            reg.unregister();
            console.log('SW unregistered');
        });
    });
    caches.keys().then(function (keys) {
        keys.forEach(function (key) {
            caches.delete(key);
            console.log('Cache deleted:', key);
        });
    });
}

// Rensa utgången session
(function () {
    try {
        var raw = localStorage.getItem('sb_session');
        if (!raw) return;
        var session = JSON.parse(raw);
        if (session && session.expires_at) {
            var nowInSeconds = Math.floor(Date.now() / 1000);
            if (session.expires_at < nowInSeconds) {
                localStorage.removeItem('sb_session');
                console.log('Utgången session rensad.');
            }
        }
    } catch (e) {
        localStorage.removeItem('sb_session');
        console.log('Ogiltig session rensad:', e);
    }
}());
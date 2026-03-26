// Unregister service worker on localhost (development)
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

//Clear expired session
(function () {
    try {
        var raw = localStorage.getItem('sb_session')
            || sessionStorage.getItem('sb_session');

        console.log('session-cleanup: found session?', raw ? 'YES - ' + raw.length + 'characters' : 'NO');

        if (!raw) return;

        var session = JSON.parse(raw);

        // C# serializes as PascalCase - check both variants
        var accessToken = session.AccessToken || session.access_token;

        console.log('session-cleanup: has access token?', accessToken ? 'YES' : 'NO');

        if (!accessToken) {
            localStorage.removeItem('sb_session');
            sessionStorage.removeItem('sb_session');
            console.log('session-cleanup: No access token, cleaning.');
        } else {
            console.log('session-cleanup: Session looks valid, keeping it.');
        }
    } catch (e) {
        localStorage.removeItem('sb_session');
        sessionStorage.removeItem('sb_session');
        console.log('session-cleanup: Invalid session cleared:', e);
    }
}());
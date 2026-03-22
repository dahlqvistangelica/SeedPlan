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

//Clear expired session
(function () {
    try {
        var raw = localStorage.getItem('sb_session')
            || sessionStorage.getItem('sb_session');

        console.log('session-cleanup: hittade session?', raw ? 'JA - ' + raw.length + 'tecken' : 'NEJ');

        if (!raw) return;

        var session = JSON.parse(raw);

        // C# serialiserar som PascalCase - kolla båda varianterna
        var accessToken = session.AccessToken || session.access_token;

        console.log('session-cleanup: har access token?', accessToken ? 'JA' : 'NEJ');

        if (!accessToken) {
            localStorage.removeItem('sb_session');
            sessionStorage.removeItem('sb_session');
            console.log('session-cleanup: Ingen access token, rensar.');
        } else {
            console.log('session-cleanup: Session ser giltig ut, behåller den.');
        }
    } catch (e) {
        localStorage.removeItem('sb_session');
        sessionStorage.removeItem('sb_session');
        console.log('session-cleanup: Ogiltig session rensad:', e);
    }
}());
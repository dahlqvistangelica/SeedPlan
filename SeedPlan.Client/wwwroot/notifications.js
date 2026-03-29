// Ask permission for notifications
window.requestNotificationPermission = async function () {
    if (!('Notification' in window)) {
        console.log('Notiser stöds inte i denna browser');
        return 'unsupported';
    }
    const permission = await Notification.requestPermission();
    console.log('Notistillstånd: ', permission);
    return permission;
};

// Send notification via service worker if possible otherwise directly
window.sendNotification = async function (title, body, icon) {
    if (Notification.permission !== 'granted') return;

    // Try going trough service worker first. (some browsers demand it)
    if ('serviceWorker' in navigator) {
        try {
            const reg = await navigator.serviceWorker.getRegistration();
            if (reg) {
                await reg.showNotification(title, {
                    body: body,
                    icon: icon || '/icon-512.png',
                    badge: '/icon-512.png',
                    vibrate: [200, 100, 200],
                    data: { url: '/sowings' }
                });
                return;
            }
        } catch (e) {
            console.log('Service worker notis misslyckades, försöker direkt:', e);
        }
    }

    // Fallback to direct notification
    const notification = new Notification(title, {
        body: body,
        icon: icon || '/icon-512.png',
        badge: '/icon-512.png'
    });

    notification.onclick = function () {
        window.focus();
        notification.close();
    };
};

// Check current notification permission
window.getNotificationPermission = function () {
    if (!('Notification' in window)) return 'unsupported';
    return Notification.permission;
};

//PUSHLOGIC

const VAPID_PUBLIC_KEY = 'BK-lfHPISWX5c4a0UO9SdnEh9lleH46Cv68cK5KT_ZgYkaRMKVIOP_0wJlf_d7ZdMZ98ZQocKK_haeBEQQJ3fLg';

//Converts base64 key to uint8array.
function urlBase64ToUint8Array(base64String) {
    const padding = '='.repeat((4 - base64String.length % 4) % 4);
    const base64 = (base64String + padding).replace(/-/g, '+').replace(/_/g, '/');
    const rawData = window.atob(base64);
    const outputArray = new Uint8Array(rawData.length);
    for (let i = 0; i < rawData.length; ++i) {
        outputArray[i] = rawData.charCodeAt(i);
    }
    return outputArray;
}

//Register puch-prenumeration
window.subscribeToPush = async function () {
    try {
        if (!('serviceWorker' in navigator) || !('PushManager' in window)) {
            console.log('Push-notiser stöds inte');
            return null;
        }

        let reg = await navigator.serviceWorker.getRegistration();
        if (!reg) {
            reg = await navigator.serviceWorker.register('/service-worker.js');
            console.log('Service worker registrerad');
        }

        if (reg.installing) {
            await new Promise(resolve => { reg.installing.addEventListener('statechange', e => { if (e.target.state == 'activated') resolve(); }); });
        }
        else if (reg.waiting) {
            await new Promise(resole => { reg.waiting.addEventListener('statechange', e => { if (e.target.state == 'activated') resolve(); }); });
        }

        reg = await navigator.serviceWorker.ready;

        let subscription = await reg.pushManager.getSubscription();
        if (subscription) {
            console.log('Befintlig push-prenumeration hittad');
            return JSON.stringify(subscription);
        }

        subscription = await reg.pushManager.subscribe(
            {
                userVisibleOnly: true,
                applicationServerKey: urlBase64ToUint8Array(VAPID_PUBLIC_KEY)
            }
        );

        console.log('Ny pushprenumeration skapad');
        return JSON.stringify(subscription);
    }
    catch (e) {
        console.log('Kunde inte prenumerera på push:', e);
        return null;
    }
};

window.unsubscribeFromPush = async function () {
    const reg = await navigator.serviceWorker.getRegistration();
    if (!reg) return;
    const subscription = await reg.pushManager.getSubscription();
    if (subscription) {
        await subscription.unsubscribe();
        console.log('Push-prenumeration avregistrerad');
    }

};

window.appBadging = {
    setBadge: function (count) {
        if ('setAppBadge' in navigator) {
            navigator.setAppBadge(count);
        }
    },
    clearBadge: function () {
        if ('clearAppBadge' in navigator) {
            navigator.clearAppBadge();
        }
    }
};

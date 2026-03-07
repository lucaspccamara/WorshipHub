import { initializeApp } from "firebase/app";
import { getMessaging, onBackgroundMessage } from "firebase/messaging/sw";
import { precacheAndRoute } from 'workbox-precaching';
import { firebaseConfig } from "./firebase-config";

// O VitePWA injetará o manifesto de precache aqui
precacheAndRoute(self.__WB_MANIFEST);

const firebaseApp = initializeApp(firebaseConfig);

const messaging = getMessaging(firebaseApp);

onBackgroundMessage(messaging, (payload) => {
    // Tenta extrair do payload do Firebase (estrutura pode variar se vier 'notification' ou só 'data')
    const title = payload.notification?.title || payload.data?.title || 'WorshipHub';
    const body = payload.notification?.body || payload.data?.body || 'Nova atualização no painel.';

    const notificationOptions = {
        body: body,
        icon: '/pwa-192x192.png',
        badge: '/vite.svg',
        tag: 'worship-push', // Evita duplicados em sequência
        data: { url: payload.data?.url || '/' }
    };

    self.registration.showNotification(title, notificationOptions);
});

self.addEventListener('notificationclick', function (event) {
    event.notification.close();

    if (event.notification.data && event.notification.data.url) {
        event.waitUntil(clients.openWindow(event.notification.data.url));
    } else {
        event.waitUntil(clients.openWindow('/'));
    }
});

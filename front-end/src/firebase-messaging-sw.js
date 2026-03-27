import { initializeApp } from "firebase/app";
import { getMessaging, onBackgroundMessage } from "firebase/messaging/sw";
import { precacheAndRoute } from 'workbox-precaching';
import { firebaseConfig } from "./firebase-config";

// O VitePWA injetará o manifesto de precache aqui
precacheAndRoute(self.__WB_MANIFEST);

// Garante que o novo SW assuma o controle imediatamente ao ser instalado,
// sem esperar o usuário fechar/reabrir o app.
self.addEventListener('install', (event) => {
  self.skipWaiting();
});

// Após assumir o controle, reclama todos os clientes abertos para que
// o App.vue detecte a atualização e dispare o reload.
self.addEventListener('activate', (event) => {
  event.waitUntil(self.clients.claim());
});

const firebaseApp = initializeApp(firebaseConfig);

let messaging;
try {
  messaging = getMessaging(firebaseApp);
} catch (e) {
  console.warn("Firebase Messaging is not supported in this browser SDK context.", e);
}

if (messaging) {
  onBackgroundMessage(messaging, (payload) => {
    // Tenta extrair do payload do Firebase (estrutura pode variar se vier 'notification' ou só 'data')
    const title = payload.notification?.title || payload.data?.title || 'WorshipHub';
    const body = payload.notification?.body || payload.data?.body || 'Nova atualização no painel.';

    const notificationOptions = {
      body: body,
      icon: '/logo-512x512.png',
      badge: '/logo-notification.svg',
      tag: 'worship-push', // Evita duplicados em sequência
      data: { url: payload.data?.url || '/' }
    };

    self.registration.showNotification(title, notificationOptions);
  });
}

self.addEventListener('notificationclick', function (event) {
  event.notification.close();

  if (event.notification.data && event.notification.data.url) {
    event.waitUntil(clients.openWindow(event.notification.data.url));
  } else {
    event.waitUntil(clients.openWindow('/'));
  }
});

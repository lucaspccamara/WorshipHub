import { ref } from 'vue';
import { getToken } from 'firebase/messaging';
import { messaging } from '../firebase';
import api from '../api';
import { Notify } from 'quasar';

// Chave pública (VAPID) obtida na configuração "Cloud Messaging" -> "Certificados Web Push" do Firebase Console.
// É obrigatória ser inserida pelo VITE caso contrário o Firebase não gera o token vinculado ao envio Push!
const VAPID_KEY = import.meta.env.VITE_FIREBASE_VAPID_KEY;

export function useNotifications() {
    const isSupported = ref(false);
    const permissionGranted = ref(false);

    // Checa se o browser tem suporte a tudo
    if (typeof window !== 'undefined' && 'serviceWorker' in navigator && 'PushManager' in window && 'Notification' in window) {
        isSupported.value = true;
        permissionGranted.value = Notification.permission === 'granted';
    }

    const syncToken = async (passedRegistration = null) => {
        try {
            // Se não passar registration, tenta pegar a que estiver pronta
            // Isso garante que estamos usando o worker gerenciado pelo VitePWA
            const registration = passedRegistration || (await navigator.serviceWorker.ready);

            // Extrai o Firebase FCM Token
            const currentToken = await getToken(messaging, {
                vapidKey: VAPID_KEY,
                serviceWorkerRegistration: registration
            });

            if (currentToken) {
                // Envia e persiste na nossa API .NET
                await saveTokenToBackend(currentToken);
            } else {
                console.warn('Nenhum token FCM retornado, valide a VAPID KEY.');
                Notify.create({ type: 'warning', message: 'Falha ao recuperar a Identidade Push do dispositivo.' });
            }
        } catch (err) {
            console.error('Erro ao sincronizar token:', err);
            Notify.create({ type: 'negative', message: 'Erro interno ao assinar notificações Push.' });
        }
    };

    const requestPermission = async () => {
        if (!isSupported.value) {
            Notify.create({ type: 'negative', message: 'Notificações Push não suportadas pelo seu navegador/SO.' });
            return;
        }

        try {
            // 1. Pede permissão pro User
            const permission = await Notification.requestPermission();

            if (permission === 'granted') {
                permissionGranted.value = true;

                // Garantir que o Service Worker está pronto antes de pedir o token
                // Isso evita que o Firebase tente registrar um worker padrão incorreto
                const registration = await navigator.serviceWorker.ready;

                await syncToken(registration);
            } else {
                Notify.create({ type: 'warning', message: 'Permissão de notificação negada.' });
            }
        } catch (err) {
            console.error('Erro no fluxo de permissão:', err);
        }
    };

    // Sincroniza automaticamente se já houver permissão ao carregar o composable
    if (isSupported.value && permissionGranted.value) {
        syncToken();
    }

    const saveTokenToBackend = async (token) => {
        try {
            await api.post('notification/token', { "fcmToken": token });
            Notify.create({ type: 'positive', message: 'Notificações ativadas para este dispositivo!' });
        } catch (err) {
            console.error('Falha ao espelhar FCM token para API', err);
            Notify.create({ type: 'negative', message: 'Não foi possível vincular seu dispositivo com sua conta.' });
        }
    };

    return {
        isSupported,
        permissionGranted,
        requestPermission,
        syncToken
    };
}

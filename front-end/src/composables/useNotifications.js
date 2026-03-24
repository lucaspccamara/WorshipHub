import { ref } from 'vue';
import { getToken, onMessage } from 'firebase/messaging';
import { useRouter } from 'vue-router';
import { messaging } from '../firebase';
import api from '../api';
import { Notify } from 'quasar';

// Chave pública (VAPID) obtida na configuração "Cloud Messaging" -> "Certificados Web Push" do Firebase Console.
// É obrigatória ser inserida pelo VITE caso contrário o Firebase não gera o token vinculado ao envio Push!
const VAPID_KEY = import.meta.env.VITE_FIREBASE_VAPID_KEY;

export function useNotifications() {
    const router = useRouter();
    const isSupported = ref(false);
    const permissionGranted = ref(false);

    // Checa se o browser tem suporte a tudo
    if (typeof window !== 'undefined' && 'serviceWorker' in navigator && 'PushManager' in window && 'Notification' in window) {
        isSupported.value = true;
        permissionGranted.value = Notification.permission === 'granted';
    }

    const syncToken = async (passedRegistration = null, showNotify = true) => {
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
                await saveTokenToBackend(currentToken, showNotify);
            } else {
                console.warn('Nenhum token FCM retornado, valide a VAPID KEY.');
                if (showNotify) {
                    Notify.create({ type: 'warning', message: 'Falha ao recuperar a Identidade Push do dispositivo.' });
                }
            }
        } catch (err) {
            console.error('Erro ao sincronizar token:', err);
            if (showNotify) {
                Notify.create({ type: 'negative', message: 'Erro interno ao assinar notificações Push.' });
            }
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

                await syncToken(registration, true);
            } else {
                Notify.create({ type: 'warning', message: 'Permissão de notificação negada.' });
            }
        } catch (err) {
            console.error('Erro no fluxo de permissão:', err);
        }
    };

    // Listener para mensagens em PRIMEIRO PLANO (aba aberta e ativa)
    if (messaging) {
        onMessage(messaging, (payload) => {
            const title = payload.notification?.title || payload.data?.title || 'WorshipHub';
            const body = payload.notification?.body || payload.data?.body || '';

            // Em foreground, geralmente mostramos um Notify interno ou forçamos a Notificação de sistema
            Notify.create({
                type: 'info',
                message: `<strong>${title}</strong><br>${body}`,
                html: true,
                position: 'top',
                timeout: 5000,
                actions: [{ label: 'Ver', color: 'white', handler: () => { router.push(payload.data?.url || '/') } }]
            });
        });
    }

    const saveTokenToBackend = async (token, showNotify = true) => {
        try {
            await api.post('notification/token', { "fcmToken": token });
            if (showNotify) {
                Notify.create({ type: 'positive', message: 'Notificações ativadas para este dispositivo!' });
            }
        } catch (err) {
            console.error('Falha ao espelhar FCM token para API', err);
            if (showNotify) {
                Notify.create({ type: 'negative', message: 'Não foi possível vincular seu dispositivo com sua conta.' });
            }
        }
    };

    // Sincroniza automaticamente se já houver permissão ao carregar o composable
    // Usamos um delay ou navigator.serviceWorker.ready para garantir que o worker subiu antes
    if (typeof window !== 'undefined' && isSupported.value && permissionGranted.value) {
        navigator.serviceWorker.ready.then(reg => syncToken(reg, false));
    };

    return {
        isSupported,
        permissionGranted,
        requestPermission,
        syncToken
    };
}

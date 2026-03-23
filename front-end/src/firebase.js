import { initializeApp } from "firebase/app";
import { getMessaging, getToken, onMessage } from "firebase/messaging";
import { firebaseConfig } from "./firebase-config";

const app = initializeApp(firebaseConfig);
let messaging = null;

try {
  messaging = getMessaging(app);
} catch (error) {
  console.warn("Firebase Messaging is not supported:", error);
}

export { app, messaging, getToken, onMessage };

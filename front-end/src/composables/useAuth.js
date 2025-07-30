import { ref } from 'vue';
import { Cookies } from 'quasar';
import { jwtDecode } from "jwt-decode";

const userId = ref('');
const userName = ref('');
const userRole = ref('');
const userEmail = ref('');
const userToken = ref(null);

const loadUserFromToken = () => {
  const token = Cookies.get('user_token');
  if (token) {
    try {
      const decoded = jwtDecode(token);
      userId.value = decoded.nameid;
      userName.value = decoded.unique_name;
      userRole.value = decoded.role;
      userEmail.value = decoded.email;
      userToken.value = token;
    } catch (error) {
      console.error('Erro ao decodificar o token:', error);
      clearUser();
    }
  } else {
    clearUser();
  }
};

const setToken = (token) => {
  Cookies.set('user_token', token, { expires: '6h' });
  loadUserFromToken();
  window.dispatchEvent(new Event('user-logged-in'));
};

const clearUser = () => {
  Cookies.remove('user_token');
  userId.value = '';
  userName.value = '';
  userRole.value = '';
  userEmail.value = '';
  userToken.value = null;
  window.dispatchEvent(new Event('user-logged-out'));
};

const hasRole = (role) => userRole.value === role;
const hasAnyRole = (roles) => roles.includes(userRole.value);
const isUserEmail = (email) => userEmail.value === email;

export const useAuth = () => ({
  userId,
  userName,
  userRole,
  userEmail,
  userToken,
  loadUserFromToken,
  setToken,
  clearUser,
  hasRole,
  hasAnyRole,
  isUserEmail
});

import { createRouter, createWebHistory } from 'vue-router';
import { useAuthStore } from './stores/authStore';
import api from './api';
import { Role } from './constants/Role';

const routes = [
  { 
    path: '/login',
    name: 'Login',
    component: () => import('./pages/LoginPage.vue')
  },
  {
    path: '/request-password-reset-code',
    name: 'Request_Password_Reset_Code',
    component: () => import('./pages/RequestPasswordResetCode.vue')
  },
  {
    path: '/verify-reset-code',
    name: 'Verify_Reset_Code',
    component: () => import('./pages/VerifyResetCode.vue')
  },
  {
    path: '/reset-password',
    name: 'Reset_Password',
    component: () => import('./pages/ResetPassword.vue')
  },
  { 
    path: '/',
    name: 'Home',
    component: () => import('./components/HomePage.vue'),
    meta: { requiresAuth: true }
  },
  { 
    path: '/schedule',
    name: 'Schedule',
    component: () => import('./pages/Schedule.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/users',
    name: 'Users',
    component: () => import('./pages/Users.vue'),
    meta: { requiresAuth: true, roles: [Role.Admin, Role.Leader] }
  },
  { 
    path: '/profile',
    name: 'Profile',
    component: () => import('./components/Profile.vue'),
    meta: { requiresAuth: true }
  },
];

const router = createRouter({
  history: createWebHistory(),
  linkActiveClass: 'active',
  routes
});

router.beforeEach(async (to, from, next) => {
  const authStore = useAuthStore();

  // Se não carregamos usuário ainda, tentar buscar com /auths/me
  if (!authStore.isAuthenticated) {
    try {
      const userResponse = await api.get('auths/me');
      authStore.setUser(userResponse.data);
    } catch {
      authStore.clearUser();
    }
  }

  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    next('/login');
  } else if (to.meta.roles && !authStore.hasAnyRole(to.meta.roles)) {
    next('/');
  } else if (to.path === '/login' && authStore.isAuthenticated) {
    next('/');
  } else {
    next();
  }
});

export default router;
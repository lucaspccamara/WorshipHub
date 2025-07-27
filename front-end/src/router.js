import { createRouter, createWebHistory } from 'vue-router'
import { Cookies } from 'quasar'
import { jwtDecode } from 'jwt-decode'
import { Role } from './constants/Role';

function hasRequiredRole(decoded, requiredRoles) {
  if (!decoded || !decoded.role) return false;
  return requiredRoles.some(role => decoded.role.includes(role));
}

const routes = [
  { 
    path: '/login',
    name: 'Login',
    component: () => import('./pages/LoginPage.vue')
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
    meta: { requiresAuth: true,  }
  },
  {
    path: '/users',
    name: 'Users',
    component: () => import('./pages/Users.vue'),
    meta: { requiresAuth: true, roles: [Role.Admin, Role.Leader] }
  }
];

const router = createRouter({
  history: createWebHistory(),
  linkActiveClass: 'active',
  routes
});

router.beforeEach((to, from, next) => {
  const token = Cookies.get('user_token')
  let isAuthenticated = false
  let decodedToken = null;

  if (token) {
    try {
      // Verifique o token usando a chave pública
      decodedToken = jwtDecode(token);
      
      // Verifica se o token ainda está válido
      isAuthenticated = decodedToken && decodedToken.exp * 1000 > Date.now();
    } catch (error) {
      console.error('Token verification failed:', error);
    }
  }

  // Impede usuário autenticado de acessar /login
  if (to.path === '/login' && isAuthenticated) {
    next('/');
    return;
  }

  if (to.meta.requiresAuth && !isAuthenticated) {
    // Se a rota requer autenticação e o usuário não está autenticado, redirecione para a página de login
    next('/login');
  } else if (to.meta.roles && !hasRequiredRole(decodedToken, to.meta.roles)) {
    // Redireciona para a página home se não tiver a função necessária
    next('/');
  } else {
    // Caso contrário, permita o acesso à rota
    next();
  }
});

export default router;
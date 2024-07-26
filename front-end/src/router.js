import { createRouter, createWebHistory } from 'vue-router'
import { Cookies } from 'quasar'
import { jwtDecode } from 'jwt-decode'

function hasRequiredRole(token, roles) {
  if (!token)
    return false; // Se não houver token, o usuário não tem a função necessária
  
  const decodedToken = jwtDecode(token);

  return roles.some((role) => decodedToken.role.includes(role));
};

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
    meta: { requiresAuth: true, roles: ['Admin'] } // Adição de role temporária para testes. REMOVER
  },
  { 
    path: '/schedule',
    name: 'Schedule',
    component: () => import('./pages/Schedule.vue'),
    meta: { requiresAuth: true, roles: ['Admin'] } // Adição de role temporária para testes. REMOVER
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

  if (token) {
    const decoded = jwtDecode(token);
    if (decoded.exp * 1000 > Date.now())
      isAuthenticated = true
  }

  if (to.meta.requiresAuth && !isAuthenticated) {
    // Se a rota requer autenticação e o usuário não está autenticado, redirecione para a página de login
    next('/login');
  } else if (to.meta.roles && !hasRequiredRole(token, to.meta.roles)) {
    // Redireciona para uma página de acesso negado se não tiver a função necessária
    next('/login');
  } else {
    // Caso contrário, permita o acesso à rota
    next();
  }
});

export default router;
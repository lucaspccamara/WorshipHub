import { createRouter, createWebHistory } from 'vue-router'
import { Cookies } from 'quasar'
import { jwtDecode } from 'jwt-decode'

const routes = [
  { 
    path: '/login',
    name: 'Login',
    component: () => import('./pages/LoginPage.vue')
  },
  { 
    path: '/',
    name: 'Home',
    component: () => import('./pages/Home.vue'),
    meta: { requiresAuth: true } }
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
  } else {
    // Caso contrário, permita o acesso à rota
    next();
  }
});

export default router;
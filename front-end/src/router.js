import { createRouter, createWebHistory } from 'vue-router';
import Login from './pages/LoginPage.vue';

const routes = [
  { path: '/', name: 'Login', component: Login }
];

const router = createRouter({
  history: createWebHistory(),
  linkActiveClass: 'active',
  routes
});

export default router;
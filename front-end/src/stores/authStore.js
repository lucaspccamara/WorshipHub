import { defineStore } from 'pinia';

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: null, // { userId, name, email, role }
  }),
  getters: {
    userId: (state) => state.user?.userId,
    isUserEmail: (state) => (email) => state.user?.email === email,
    isAuthenticated: (state) => state.user !== null,
    hasRole: (state) => (role) => state.user?.role === role,
    hasAnyRole: (state) => (roles) => roles.includes(state.user?.role),
  },
  actions: {
    setUser(userData) {
      this.user = userData;
    },
    clearUser() {
      this.user = null;
    }
  }
});

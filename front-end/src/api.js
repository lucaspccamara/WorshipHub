import axios from 'axios';
import { Cookies, Notify } from 'quasar'

const api = axios.create({
  timeout: 90000,
  baseURL: import.meta.env.VITE_API_URL,
  headers: { 'Content-Type': 'application/json', 'Accept': 'application/json' },
});

api.interceptors.request.use((config) => {
  config.headers['Authorization'] = `Bearer ${Cookies.get('user_token')}`
  return config
}, (error) => {
  return Promise.reject(error)
});

const apiMethods = {
  // Métodos REST padrão
  getAll: async (resource) => {
    try {
      const response = await api.get(`/${resource}`);
      return response;
    } catch (error) {
      throw new Error(error.response.data || error.message);
    }
  },
  getOne: async (resource, id) => {
    try {
      const response = await api.get(`/${resource}/${id}`);
      return response;
    } catch (error) {
      throw new Error(error.response.data || error.message);
    }
  },
  post: async (resource, data) => {
    try {
      const response = await api.post(`/${resource}`, data);
      return response;
    } catch (error) {
      throw new Error(error.response.data || error.message);
    }
  },
  put: async (resource, id, data) => {
    try {
      const response = await api.put(`/${resource}/${id}`, data);
      return response;
    } catch (error) {
      throw new Error(error.response.data || error.message);
    }
  },
  delete: async (resource, id) => {
    try {
      const response = await api.delete(`/${resource}/${id}`);
      return response;
    } catch (error) {
      throw new Error(error.response.data || error.message);
    }
  },

  // Outros métodos
  getPost: async (resource, data) => {
    try {
      const response = await api.post(`/${resource}`, data);
      return response;
    } catch (error) {
      Notify.create({
        message: error.response.data || error.message,
        color: 'negative'
      });
      throw new Error(error.response.data || error.message);
    }
  }
};

export default apiMethods;

import axios from 'axios';
import { Notify } from 'quasar';

const api = axios.create({
  timeout: 90000,
  baseURL: import.meta.env.VITE_API_URL,
  headers: { 'Content-Type': 'application/json', 'Accept': 'application/json' },
  withCredentials: true
});

const apiMethods = {
  // Métodos REST padrão
  get: async (resource, id) => {
    try {
      const response = await api.get(`/${resource}${id != null ? '/' + id : ''}`);
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
      throw new Error(error.response.data || error.message);
    }
  }
};

export default apiMethods;

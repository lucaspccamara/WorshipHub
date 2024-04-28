import axios from 'axios';

const api = axios.create({
  timeout: 600,
  baseURL: import.meta.env.VITE_API_URL,
  headers: { 'Content-Type': 'application/json', 'Accept': 'application/json' },
});

const apiMethods = {
  // Métodos REST padrão
  getAll: async (resource) => {
    try {
      const response = await api.get(`/${resource}`);
      return response.data;
    } catch (error) {
      throw new Error(error.response.data.message || error.message);
    }
  },
  getOne: async (resource, id) => {
    try {
      const response = await api.get(`/${resource}/${id}`);
      return response.data;
    } catch (error) {
      throw new Error(error.response.data.message || error.message);
    }
  },
  create: async (resource, data) => {
    try {
      const response = await api.post(`/${resource}`, data);
      return response.data;
    } catch (error) {
      throw new Error(error.response.data.message || error.message);
    }
  },
  update: async (resource, id, data) => {
    try {
      const response = await api.put(`/${resource}/${id}`, data);
      return response.data;
    } catch (error) {
      throw new Error(error.response.data.message || error.message);
    }
  },
  remove: async (resource, id) => {
    try {
      const response = await api.delete(`/${resource}/${id}`);
      return response.data;
    } catch (error) {
      throw new Error(error.response.data.message || error.message);
    }
  },

  // Outros métodos
  getPost: async (resource, data) => {
    try {
      const response = await api.post(`/${resource}`, data);
      return response.data;
    } catch (error) {
      throw new Error(error.response.data.message || error.message);
    }
  }
};

export default apiMethods;

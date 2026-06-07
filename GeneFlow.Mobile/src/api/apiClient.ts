import axios from 'axios';
import { config } from '../config/env';

const apiClient = axios.create({
    baseURL: config.apiBaseUrl,
    headers: {
        'Content-Type': 'application/json',
    },
});

// Attach JWT token to every request
apiClient.interceptors.request.use((requestConfig) => {
    const token = localStorage.getItem('geneflow_token');
    if (token) {
        requestConfig.headers.Authorization = `Bearer ${token}`;
    }
    return requestConfig;
});

// Handle 401 globally — clear token and redirect to login
apiClient.interceptors.response.use(
    (response) => response,
    (error) => {
        if (error.response?.status === 401) {
            localStorage.removeItem('geneflow_token');
            localStorage.removeItem('geneflow_user');
            window.location.href = '/login';
        }
        return Promise.reject(error);
    }
);

export default apiClient;

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5016';

export const config = {
    apiBaseUrl: `${API_BASE_URL}/api`,
};

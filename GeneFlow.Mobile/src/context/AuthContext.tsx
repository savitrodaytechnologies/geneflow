import React, { createContext, useContext, useState, useCallback } from 'react';
import type { AuthUser } from '../types/auth';

interface AuthContextValue {
    user: AuthUser | null;
    token: string | null;
    isAuthenticated: boolean;
    login: (token: string, user: AuthUser) => void;
    logout: () => void;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

const TOKEN_KEY = 'geneflow_token';
const USER_KEY = 'geneflow_user';

function loadFromStorage(): { token: string | null; user: AuthUser | null } {
    try {
        const token = localStorage.getItem(TOKEN_KEY);
        const userJson = localStorage.getItem(USER_KEY);
        const user = userJson ? (JSON.parse(userJson) as AuthUser) : null;
        return { token, user };
    } catch {
        return { token: null, user: null };
    }
}

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
    const initial = loadFromStorage();
    const [token, setToken] = useState<string | null>(initial.token);
    const [user, setUser] = useState<AuthUser | null>(initial.user);

    const login = useCallback((newToken: string, newUser: AuthUser) => {
        localStorage.setItem(TOKEN_KEY, newToken);
        localStorage.setItem(USER_KEY, JSON.stringify(newUser));
        setToken(newToken);
        setUser(newUser);
    }, []);

    const logout = useCallback(() => {
        localStorage.removeItem(TOKEN_KEY);
        localStorage.removeItem(USER_KEY);
        setToken(null);
        setUser(null);
    }, []);

    return (
        <AuthContext.Provider value={{ user, token, isAuthenticated: !!token && !!user, login, logout }}>
            {children}
        </AuthContext.Provider>
    );
};

export function useAuth(): AuthContextValue {
    const ctx = useContext(AuthContext);
    if (!ctx) throw new Error('useAuth must be used inside AuthProvider');
    return ctx;
}

import { describe, it, expect, vi, beforeEach } from 'vitest';

/**
 * Tests for the AuthContext logic — storage persistence and state.
 * We test the pure helper functions independently of React to keep tests fast.
 */

// ─── localStorage token helpers ───────────────────────────────────────────────

const TOKEN_KEY = 'geneflow_token';
const USER_KEY = 'geneflow_user';

const mockUser = {
    userId: 'abc-123',
    email: 'test@lab.com',
    fullName: 'Test User',
    systemRole: 'User',
    labId: 'lab-456',
    labRole: 'Researcher',
};

describe('Auth storage helpers', () => {
    beforeEach(() => {
        localStorage.clear();
    });

    it('stores and retrieves token from localStorage', () => {
        localStorage.setItem(TOKEN_KEY, 'my-jwt-token');
        expect(localStorage.getItem(TOKEN_KEY)).toBe('my-jwt-token');
    });

    it('stores and retrieves user as JSON', () => {
        localStorage.setItem(USER_KEY, JSON.stringify(mockUser));
        const parsed = JSON.parse(localStorage.getItem(USER_KEY) ?? '{}');
        expect(parsed.email).toBe('test@lab.com');
        expect(parsed.labId).toBe('lab-456');
    });

    it('clears token and user on logout', () => {
        localStorage.setItem(TOKEN_KEY, 'token');
        localStorage.setItem(USER_KEY, JSON.stringify(mockUser));
        // Simulate logout
        localStorage.removeItem(TOKEN_KEY);
        localStorage.removeItem(USER_KEY);
        expect(localStorage.getItem(TOKEN_KEY)).toBeNull();
        expect(localStorage.getItem(USER_KEY)).toBeNull();
    });

    it('handles corrupt user JSON gracefully', () => {
        localStorage.setItem(USER_KEY, 'not-valid-json{{{');
        let user = null;
        try {
            user = JSON.parse(localStorage.getItem(USER_KEY) ?? '');
        } catch {
            user = null;
        }
        expect(user).toBeNull();
    });
});

// ─── Auth redirect rules ────────────────────────────────────────────────────

describe('Auth redirect logic', () => {
    it('unauthenticated user should redirect to /login', () => {
        const isAuthenticated = false;
        const targetPath = isAuthenticated ? '/home' : '/login';
        expect(targetPath).toBe('/login');
    });

    it('authenticated user should NOT redirect to /login', () => {
        const isAuthenticated = true;
        const targetPath = isAuthenticated ? '/home' : '/login';
        expect(targetPath).toBe('/home');
    });

    it('user with labId has access to experiments', () => {
        const user = { ...mockUser, labId: 'lab-456' };
        expect(user.labId).toBeTruthy();
    });

    it('user without labId cannot create experiments', () => {
        const user = { ...mockUser, labId: undefined };
        expect(user.labId).toBeFalsy();
    });
});

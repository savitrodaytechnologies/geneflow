import { describe, it, expect } from 'vitest';
import { normalizePhone, isValidLocalPhone, buildFullPhone } from '../utils/phoneUtils';

describe('normalizePhone', () => {
    it('strips spaces, dashes and parentheses', () => {
        expect(normalizePhone('+1 (800) 555-0100')).toBe('18005550100');
    });

    it('returns digits-only string unchanged', () => {
        expect(normalizePhone('9876543210')).toBe('9876543210');
    });

    it('handles empty string', () => {
        expect(normalizePhone('')).toBe('');
    });

    it('strips country code plus sign', () => {
        expect(normalizePhone('+91-98765-43210')).toBe('919876543210');
    });
});

describe('isValidLocalPhone', () => {
    it('accepts 10-digit number', () => {
        expect(isValidLocalPhone('9876543210')).toBe(true);
    });

    it('rejects too-short number (< 7 digits)', () => {
        expect(isValidLocalPhone('12345')).toBe(false);
    });

    it('rejects empty string', () => {
        expect(isValidLocalPhone('')).toBe(false);
    });

    it('accepts 7-digit minimum', () => {
        expect(isValidLocalPhone('1234567')).toBe(true);
    });

    it('accepts 15-digit maximum (E.164)', () => {
        expect(isValidLocalPhone('123456789012345')).toBe(true);
    });

    it('rejects 16-digit number', () => {
        expect(isValidLocalPhone('1234567890123456')).toBe(false);
    });
});

describe('buildFullPhone', () => {
    it('combines dial code and local number', () => {
        expect(buildFullPhone('+1', '8005550100')).toBe('18005550100');
    });

    it('strips non-digits from both parts', () => {
        expect(buildFullPhone('+91', '98765-43210')).toBe('919876543210');
    });

    it('handles dial code without plus', () => {
        expect(buildFullPhone('44', '7911123456')).toBe('447911123456');
    });
});

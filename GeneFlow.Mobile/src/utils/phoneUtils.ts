/**
 * Phone normalization utility used in LoginPage and RegisterLabPage.
 * Extracted here so it can be tested independently.
 */

/** Strips all non-digit characters from a phone string. */
export const normalizePhone = (value: string): string => value.replace(/\D/g, '');

/** Returns true when a local phone number looks valid (7–15 digits). */
export const isValidLocalPhone = (digits: string): boolean =>
    digits.length >= 7 && digits.length <= 15;

/** Combines dial code and local number into a full phone string. */
export const buildFullPhone = (dialCode: string, localDigits: string): string =>
    `${dialCode.replace(/\D/g, '')}${localDigits.replace(/\D/g, '')}`;

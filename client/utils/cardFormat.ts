/**
 * Input formatting for the new-card fields of the invoice payment page.
 *
 * Each function takes whatever the person typed and returns what the field should then show:
 * digits only, grouped the way a card prints them. They never validate; the payment provider
 * does that.
 *
 * @module utils/cardFormat
 */

/** Digits in a card number. */
const CARD_NUMBER_DIGITS = 16

/** Digits in an expiry date (month and two-digit year). */
const EXPIRY_DIGITS = 4

/** Digits in a security code. */
const CVV_DIGITS = 4

/**
 * Keeps digits only and cuts them to a length.
 *
 * @param raw - What the person typed.
 * @param max - The most digits to keep.
 * @returns The digits.
 */
const digitsOf = (raw: string, max: number): string => raw.replace(/\D/g, '').slice(0, max)

/**
 * Formats a card number as groups of four digits.
 *
 * @param raw - What the person typed.
 * @returns Up to sixteen digits, a space after every fourth.
 */
export const formatCardNumber = (raw: string): string => digitsOf(raw, CARD_NUMBER_DIGITS).replace(/(.{4})/g, '$1 ').trim()

/**
 * Formats an expiry date as `MM / YY`.
 *
 * @param raw - What the person typed.
 * @returns The month, and from the third digit on a separator and the year.
 */
export const formatExpiry = (raw: string): string => {
  const digits = digitsOf(raw, EXPIRY_DIGITS)
  return digits.length >= 3 ? `${digits.slice(0, 2)} / ${digits.slice(2)}` : digits
}

/**
 * Formats a security code.
 *
 * @param raw - What the person typed.
 * @returns Up to four digits.
 */
export const formatCvv = (raw: string): string => digitsOf(raw, CVV_DIGITS)

/**
 * Client-side checks of the hosting setup wizard.
 *
 * They mirror the server's `SetupServiceValidator` so provisioning is never reached with a
 * malformed value, but the server stays the authority: each function only decides *whether* a
 * typed value is acceptable and returns the i18n key of the reason, never a sentence. Every
 * function treats an empty value as "no error yet", so an untouched form shows its placeholder
 * instead of a complaint; the wizard's Continue button is disabled on emptiness separately.
 *
 * @module utils/hostingSetup
 */

/** Shortest control-panel username the server accepts. */
export const USERNAME_MIN_LENGTH = 3

/** Longest control-panel username the server accepts. */
export const USERNAME_MAX_LENGTH = 8

/** Shortest control-panel password the server accepts. */
export const PASSWORD_MIN_LENGTH = 8

/** A username starts with a letter and holds lowercase letters and digits only. */
const USERNAME_FORMAT = /^[a-z][a-z0-9]*$/

/** Labels of a-z/0-9/hyphen (no leading or trailing hyphen), at least two of them, TLD of 2+ letters. */
const DOMAIN_FORMAT = /^(?=.{1,253}$)([a-z0-9](-*[a-z0-9])*\.)+[a-z]{2,}$/i

/**
 * Cleans a username as it is typed: lowercase letters and digits only.
 *
 * @param raw - What the person typed.
 * @returns The value the field should show.
 */
export const sanitizeUsername = (raw: string): string => raw.toLowerCase().replace(/[^a-z0-9]/g, '')

/**
 * Names why a username is not acceptable.
 *
 * @param username - The typed username.
 * @returns An i18n key, or an empty string when the value is empty or acceptable.
 */
export const usernameErrorKey = (username: string): string => {
  if (!username) {
    return ''
  }
  if (!USERNAME_FORMAT.test(username)) {
    return 'client.setup.usernameFormat'
  }
  if (username.length < USERNAME_MIN_LENGTH) {
    return 'client.setup.usernameMin'
  }
  if (username.length > USERNAME_MAX_LENGTH) {
    return 'client.setup.usernameMax'
  }
  return ''
}

/**
 * Names why a domain is not a valid hostname, for example `notadomain` with no dot.
 *
 * @param domain - The typed domain.
 * @returns An i18n key, or an empty string when the value is empty or acceptable.
 */
export const domainErrorKey = (domain: string): string => {
  const trimmed = domain.trim()
  if (!trimmed) {
    return ''
  }
  return DOMAIN_FORMAT.test(trimmed) ? '' : 'client.setup.domainInvalid'
}

/**
 * Names why a password is too weak for the hosting control panel.
 *
 * @param password - The typed password.
 * @returns An i18n key, or an empty string when the value is empty or long enough.
 */
export const passwordErrorKey = (password: string): string => {
  if (!password) {
    return ''
  }
  return password.length < PASSWORD_MIN_LENGTH ? 'client.setup.passwordMin' : ''
}

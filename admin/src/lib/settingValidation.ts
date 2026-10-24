/**
 * Client-side checks for the free-text storefront settings that have a shape.
 *
 * The API refuses the same shapes (`PortalSettingKeys.Patterns`), so these only spare the operator
 * a round trip; they are kept in step with the backend patterns by hand. An empty value always
 * passes, because empty means "unset" and hides the item on the storefront.
 */

/** The shapes a storefront text setting can be required to have. */
export enum SettingFieldRule {
  /** One e-mail address. */
  Email = 'email',
  /** An address that starts with `https://`. */
  HttpsUrl = 'httpsUrl',
}

/** One e-mail address: no spaces, one `@`, a dotted domain. Mirrors the backend pattern. */
const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

/** An `https://` host with an optional path and no spaces. Mirrors the backend pattern. */
const HTTPS_URL_PATTERN = /^https:\/\/[^\s/]+(\/\S*)?$/

/** The pattern each rule requires. */
const RULE_PATTERNS: Record<SettingFieldRule, RegExp> = {
  [SettingFieldRule.Email]: EMAIL_PATTERN,
  [SettingFieldRule.HttpsUrl]: HTTPS_URL_PATTERN,
}

/**
 * Whether a typed value satisfies a rule.
 *
 * @param rule - The shape the setting requires.
 * @param value - What the operator typed; surrounding spaces are ignored, as the API trims them.
 * @returns True for an empty value or one that has the shape.
 */
export const isValidSettingValue = (rule: SettingFieldRule, value: string): boolean => {
  const trimmed = value.trim()
  return trimmed === '' || RULE_PATTERNS[rule].test(trimmed)
}

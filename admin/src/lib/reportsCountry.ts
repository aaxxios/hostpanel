/**
 * Country names for the reports, from the browser locale data instead of a hand-kept table.
 */

/** Code the reports use for clients with no known country. */
export const UNKNOWN_COUNTRY = 'Unknown'

/**
 * Names a country by its ISO-2 code in the given language.
 *
 * @param code - ISO-2 country code, or `Unknown`.
 * @param locale - Language to name it in.
 * @returns The country name; the code itself when the browser does not know it.
 */
export const reportCountryName = (code: string, locale: string): string => {
  if (code === UNKNOWN_COUNTRY) {
    return code
  }
  try {
    return new Intl.DisplayNames([locale], { type: 'region' }).of(code) ?? code
  } catch {
    // A malformed code makes Intl throw; the code is the best text left.
    return code
  }
}

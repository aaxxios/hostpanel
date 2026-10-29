/**
 * Display fallbacks for values the API leaves empty because they are genuinely optional (a ticket
 * reply with no author, a field nobody filled in) and for the registration period in years.
 *
 * The backend never invents a placeholder; every screen shows its own, and they all come from here
 * so the wording is the same on every list, detail and report.
 */
import { useI18n } from 'vue-i18n'

/** What {@link useFallbackLabels} hands back. */
export interface FallbackLabels {
  /** Value, or "Unknown" when it is unresolved. */
  orUnknown: (value: string | null | undefined) => string
  /** Value, or "N/A" when it is empty. */
  orNotAvailable: (value: string | null | undefined) => string
  /** "1 year" / "N years" for a positive count, "N/A" when the count is 0, negative or missing. */
  yearsLabel: (years: number | null | undefined) => string
}

/**
 * Creates the fallback formatters bound to the active locale.
 *
 * @returns The formatters; each is pure and safe to call from a template.
 */
export const useFallbackLabels = (): FallbackLabels => {
  const { t } = useI18n()

  /**
   * Whether a text is present.
   *
   * @param value - Text from the API.
   * @returns False for null, undefined and blank text.
   */
  const hasText = (value: string | null | undefined): value is string => {
    return typeof value === 'string' && value.trim() !== ''
  }

  /**
   * Value, or "Unknown" when unresolved.
   *
   * @param value - The value to show.
   * @returns The value or the localised "Unknown".
   */
  const orUnknown = (value: string | null | undefined): string => {
    return hasText(value) ? value : t('shared.unknown')
  }

  /**
   * Value, or "N/A" when empty.
   *
   * @param value - The value to show.
   * @returns The value or the localised "N/A".
   */
  const orNotAvailable = (value: string | null | undefined): string => {
    return hasText(value) ? value : t('shared.notAvailable')
  }

  /**
   * Pluralised years, "N/A" for an unknown period.
   *
   * @param years - Number of years.
   * @returns The pluralised label or the localised "N/A".
   */
  const yearsLabel = (years: number | null | undefined): string => {
    if (typeof years !== 'number' || years <= 0) {
      return t('shared.notAvailable')
    }
    return t('shared.years', { n: years }, years)
  }

  return { orUnknown, orNotAvailable, yearsLabel }
}

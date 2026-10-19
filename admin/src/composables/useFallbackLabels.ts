/**
 * Display fallbacks for fields the API leaves `null` when it cannot resolve them (client name,
 * product name, service name, country) and for the registration period in years.
 *
 * The backend never invents a placeholder; every screen shows its own, and they all come from here
 * so the wording is the same on every list, detail and report.
 */
import { useI18n } from 'vue-i18n'

/** What {@link useFallbackLabels} hands back. */
export interface FallbackLabels {
  /** Name, or "Client #id" when the name is unresolved and an id is known, else "Unknown". */
  clientLabel: (name: string | null | undefined, id?: number | null) => string
  /** Name, or "Product #id" when the name is unresolved and an id is known, else "Unknown". */
  productLabel: (name: string | null | undefined, id?: number | null) => string
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
   * Whether an id can be shown.
   *
   * @param id - Entity id.
   * @returns True for a positive number.
   */
  const hasId = (id: number | null | undefined): id is number => {
    return typeof id === 'number' && id > 0
  }

  /**
   * Name, or "Client #id" / "Unknown" when unresolved.
   *
   * @param name - Resolved client name.
   * @param id - Client id used for the fallback.
   * @returns The label to show.
   */
  const clientLabel = (name: string | null | undefined, id?: number | null): string => {
    if (hasText(name)) {
      return name
    }
    return hasId(id) ? t('shared.clientFallback', { id }) : t('shared.unknown')
  }

  /**
   * Name, or "Product #id" / "Unknown" when unresolved.
   *
   * @param name - Resolved product name.
   * @param id - Product id used for the fallback.
   * @returns The label to show.
   */
  const productLabel = (name: string | null | undefined, id?: number | null): string => {
    if (hasText(name)) {
      return name
    }
    return hasId(id) ? t('shared.productFallback', { id }) : t('shared.unknown')
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

  return { clientLabel, productLabel, orUnknown, orNotAvailable, yearsLabel }
}

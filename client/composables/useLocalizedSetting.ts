/**
 * Reads an operator setting that can differ per language.
 *
 * Some settings (the About story, the office address, the working hours) are written by the
 * operator in each language they publish. They are stored as a default row plus a Russian and an
 * Armenian row; English visitors, and any visitor whose language has no row of its own, get the
 * default. One reader holds that rule so no page restates it.
 *
 * @module composables/useLocalizedSetting
 */

import type { PortalSettingKey } from '~/constants/portalSettingKey'
import { AppLocale } from '~/types/applocale'

/** The rows that hold one text in each language, with the runtime-config key each falls back to. */
export interface LocalizedSettingSpec {
  /** The default-language row and its runtime-config key. */
  default: { key: PortalSettingKey, envKey: string }
  /** The Russian row and its runtime-config key. */
  ru: { key: PortalSettingKey, envKey: string }
  /** The Armenian row and its runtime-config key. */
  hy: { key: PortalSettingKey, envKey: string }
}

/**
 * Creates the per-language reader.
 *
 * @returns A function that resolves a {@link LocalizedSettingSpec} for the active language.
 */
export const useLocalizedSetting = () => {
  const { get } = usePortalSettings()
  const { locale } = useI18n()

  /**
   * Reads the text in the active language, falling back to the default one.
   *
   * @param spec The default, Russian and Armenian rows of the setting.
   * @returns The text, trimmed; empty when nothing is set. Reactive when read inside a computed.
   */
  const getLocalized = (spec: LocalizedSettingSpec): string => {
    const own = locale.value === AppLocale.Ru ? spec.ru : locale.value === AppLocale.Hy ? spec.hy : undefined
    const perLocale = own ? get(own.key, own.envKey) : ''
    return perLocale || get(spec.default.key, spec.default.envKey)
  }

  return { getLocalized }
}

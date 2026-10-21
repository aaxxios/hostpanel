/**
 * Which home-page sections the operator has switched on.
 *
 * Each section has a `portal.sections.*` setting holding `true` or `false`; a section whose
 * setting has no value falls back to `HOME_SECTION_DEFAULT`, so a fresh install shows
 * everything except the portfolio projects.
 *
 * @module composables/useHomeSections
 */

import { HOME_SECTION_DEFAULT, HOME_SECTION_SETTING } from '~/constants/homeSection'
import type { HomeSection } from '~/constants/homeSection'
import { SETTING_ON } from '~/constants/settingValue'

/**
 * Reads the section switches.
 *
 * @returns `isVisible`, which answers whether a section is shown.
 */
export const useHomeSections = () => {
  const { get } = usePortalSettings()

  /**
   * Whether a home-page section is shown.
   *
   * @param section - The section.
   * @returns True when the operator switched it on, or said nothing and it is on by default.
   */
  const isVisible = (section: HomeSection): boolean => {
    const value = get(HOME_SECTION_SETTING[section], '')
    if (!value) {
      return HOME_SECTION_DEFAULT[section]
    }
    return value === SETTING_ON
  }

  return { isVisible }
}

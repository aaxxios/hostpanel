/**
 * The rule that decides whether a template's home page shows its partner logos.
 *
 * @module utils/partnersVisible
 */

import { HomeSection } from '~/constants/homeSection'

/**
 * Whether the home page shows its single partners section. The partner logos and the former
 * "Trusted by" strip are one section now, so it is shown while either switch is on and hidden
 * only when both are off. Every template uses this rule.
 *
 * @param isVisible - Answers whether a home section is switched on (from `useHomeSections`).
 * @returns True when the Partners or the Trusted by switch is on.
 */
export const isPartnersVisible = (isVisible: (section: HomeSection) => boolean): boolean => {
  return isVisible(HomeSection.Partners) || isVisible(HomeSection.Trusted)
}

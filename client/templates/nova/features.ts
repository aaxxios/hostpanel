/**
 * Visibility rule for nova's capability cards: which claims this install may state.
 *
 * @module templates/nova/features
 */

import { SETTING_ON } from '~/constants/settingValue'
import type { Feature } from '~/templates/nova/types/feature'

/**
 * Filters a feature list down to the claims this install is entitled to make.
 *
 * A {@link Feature} without `requiresSetting` is always shown: those lines
 * describe what the platform itself does, and they are true of every install.
 * A feature that names a setting appears only once an operator has switched that
 * setting on (`true`), because the thing it claims — NVMe disks, a CDN, a malware
 * scanner, a backup schedule — belongs to their servers and is invisible from
 * here. The alternative was to print them regardless, which is how a hosting
 * page ends up advertising hardware nobody bought.
 *
 * Lives beside the sections rather than in `composables/` because nothing
 * outside nova needs it, and it is a composable rather than a plain function
 * because reading a setting means reading Nuxt state.
 *
 * @param features The full list from `content.ts`.
 * @returns Only the entries that should render, in their original order.
 */
export const useVisibleFeatures = (features: readonly Feature[]) => {
  const { get } = usePortalSettings()

  return computed(() => features.filter((feature) => {
    if (!feature.requiresSetting) {
      return true
    }

    // Settings only: these claims have no environment fallback to read.
    return get(feature.requiresSetting, '') === SETTING_ON
  }))
}

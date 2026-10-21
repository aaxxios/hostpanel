/**
 * The one-click installer section: how many apps it offers (the operator's count) and the
 * fixed list of categories.
 *
 * @module composables/useInstallerApps
 */

import { InstallerAppCategory } from '~/constants/installerAppCategory'
import { PortalSettingKey } from '~/constants/portalSettingKey'

/**
 * Reads the installer details.
 *
 * @returns The count as the operator wrote it (empty when unset, which hides the count line)
 * and every category.
 */
export const useInstallerApps = () => {
  const { get } = usePortalSettings()

  return computed(() => ({
    count: get(PortalSettingKey.InstallerAppCount, ''),
    categories: Object.values(InstallerAppCategory),
  }))
}

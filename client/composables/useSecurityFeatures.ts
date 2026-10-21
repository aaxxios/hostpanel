/**
 * The protections of the security section, read from the `portal.features.*` switches.
 *
 * @module composables/useSecurityFeatures
 */

import { PortalSettingKey } from '~/constants/portalSettingKey'
import { SecurityFeatureKey } from '~/constants/securityFeatureKey'
import { SETTING_ON } from '~/constants/settingValue'

/** The switch behind each protection, in the order the section lists them. */
const FEATURE_SETTING: Record<SecurityFeatureKey, PortalSettingKey> = {
  [SecurityFeatureKey.Ssl]: PortalSettingKey.FeaturesSsl,
  [SecurityFeatureKey.Backups]: PortalSettingKey.FeaturesBackups,
  [SecurityFeatureKey.Ddos]: PortalSettingKey.FeaturesDdos,
  [SecurityFeatureKey.Malware]: PortalSettingKey.FeaturesMalware,
  [SecurityFeatureKey.Firewall]: PortalSettingKey.FeaturesFirewall,
}

/**
 * Reads the security switches.
 *
 * @returns The protections the operator switched on, in enum order.
 */
export const useSecurityFeatures = () => {
  const { get } = usePortalSettings()

  return computed<SecurityFeatureKey[]>(() =>
    Object.values(SecurityFeatureKey).filter(key => get(FEATURE_SETTING[key], '') === SETTING_ON))
}

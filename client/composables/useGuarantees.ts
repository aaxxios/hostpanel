/**
 * The promises of the guarantees section, read from the `portal.migration.*` and
 * `portal.guarantee.*` settings.
 *
 * @module composables/useGuarantees
 */

import { GuaranteeKey } from '~/constants/guaranteeKey'
import { PortalSettingKey } from '~/constants/portalSettingKey'
import { SETTING_ON } from '~/constants/settingValue'
import type { GuaranteeItem } from '~/types/guaranteeitem'

/** A money-back period the operator may write: one to three digits. */
const DAYS_PATTERN = /^\d{1,3}$/

/**
 * Reads the guarantees.
 *
 * @returns The promises that are set, in enum order; the money-back one carries its days.
 */
export const useGuarantees = () => {
  const { get } = usePortalSettings()

  return computed<GuaranteeItem[]>(() => {
    const items: GuaranteeItem[] = []

    if (get(PortalSettingKey.MigrationFree, '') === SETTING_ON) {
      items.push({ key: GuaranteeKey.FreeMigration })
    }

    const days = get(PortalSettingKey.GuaranteeMoneyBackDays, '')
    if (DAYS_PATTERN.test(days) && Number(days) > 0) {
      items.push({ key: GuaranteeKey.MoneyBack, days: Number(days) })
    }

    if (get(PortalSettingKey.GuaranteeNoHiddenFees, '') === SETTING_ON) {
      items.push({ key: GuaranteeKey.NoHiddenFees })
    }

    return items
  })
}

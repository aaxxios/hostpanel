/**
 * The figures of the performance section, read from the `portal.stats.*` settings.
 *
 * Only figures the operator set are returned, so a figure with no value is simply absent and
 * the section hides itself when none is set.
 *
 * @module composables/usePerformanceStats
 */

import { PerformanceStatKey } from '~/constants/performanceStatKey'
import { PortalSettingKey } from '~/constants/portalSettingKey'
import type { PerformanceStat } from '~/types/performancestat'

/** The setting holding each figure, in the order the section lists them. */
const STAT_SETTING: Record<PerformanceStatKey, PortalSettingKey> = {
  [PerformanceStatKey.Uptime]: PortalSettingKey.StatsUptime,
  [PerformanceStatKey.ResponseTime]: PortalSettingKey.StatsResponseTime,
  [PerformanceStatKey.SitesHosted]: PortalSettingKey.StatsSitesHosted,
  [PerformanceStatKey.SupportResponse]: PortalSettingKey.StatsSupportResponse,
}

/**
 * Reads the performance figures.
 *
 * @returns The figures that have a value, in enum order.
 */
export const usePerformanceStats = () => {
  const { get } = usePortalSettings()

  return computed<PerformanceStat[]>(() =>
    Object.values(PerformanceStatKey)
      .map(key => ({ key, value: get(STAT_SETTING[key], key === PerformanceStatKey.Uptime ? 'portalStatsUptime' : '') }))
      .filter(stat => stat.value !== ''))
}

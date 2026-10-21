/**
 * Unit tests for AuroraPerformance: one card per figure the composable reports, the section hides without any, and the grid is the balanced one.
 */
import { describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import AuroraPerformance from '~/templates/aurora/sections/AuroraPerformance.vue'
import { PerformanceStatKey } from '~/constants/performanceStatKey'
import type { PerformanceStat } from '~/types/performancestat'
import { stubAuroraGlobals } from '../auroraGlobals'
import { balancedColumns } from '~/utils/balancedGrid'

/** The figures the mocked composable reports. */
const stats = vi.hoisted(() => ({ value: [] as PerformanceStat[] }))

vi.mock('~/composables/usePerformanceStats', () => ({
  /**
   * Stands in for the figures composable.
   *
   * @returns The mocked figures as a ref.
   */
  usePerformanceStats: () => ref(stats.value),
}))

describe('AuroraPerformance', () => {
  it('renders a card with the value for every figure', () => {
    stats.value = [
      { key: PerformanceStatKey.Uptime, value: '99.9%' },
      { key: PerformanceStatKey.ResponseTime, value: '180 ms' },
    ]
    stubAuroraGlobals()

    const wrapper = mountWithKit(AuroraPerformance)

    expect(wrapper.find('[data-testid="performance-value-uptime"]').text()).toBe('99.9%')
    expect(wrapper.find('[data-testid="performance-value-response_time"]').text()).toBe('180 ms')
    expect(wrapper.find('[data-testid="performance-sites_hosted"]').exists()).toBe(false)
  })

  it('renders nothing without figures', () => {
    stats.value = []
    stubAuroraGlobals()

    const wrapper = mountWithKit(AuroraPerformance)

    expect(wrapper.find('[data-testid="aurora-performance"]').exists()).toBe(false)
  })

  it('lays the cards out in the balanced grid', () => {
    stats.value = [
      { key: PerformanceStatKey.Uptime, value: '99.9%' },
      { key: PerformanceStatKey.ResponseTime, value: '180 ms' },
      { key: PerformanceStatKey.SitesHosted, value: '12,000+' },
    ]
    stubAuroraGlobals()

    const grid = mountWithKit(AuroraPerformance).find('[data-testid="aurora-performance-grid"]')

    expect(grid.attributes('data-columns')).toBe(String(balancedColumns(3)))
  })
})

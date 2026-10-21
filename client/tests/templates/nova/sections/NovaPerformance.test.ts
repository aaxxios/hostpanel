/**
 * Unit tests for NovaPerformance: the operator figures as tiles, capability cards beneath them
 * (storage, cache and CDN hidden until their setting is filled in), and the section hiding only
 * when there is neither.
 *
 * @module tests/templates/nova/sections/NovaPerformance.test
 */
import { computed } from 'vue'
import { describe, expect, it, vi } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import NovaPerformance from '~/templates/nova/sections/NovaPerformance.vue'
import { PERFORMANCE_FEATURES } from '~/templates/nova/content'
import type { Feature } from '~/templates/nova/types/feature'
import { PerformanceStatKey } from '~/constants/performanceStatKey'
import type { PerformanceStat } from '~/types/performancestat'
import { stubTemplateGlobals } from '~/tests/helpers/templateGlobals'

/**
 * Counts the features that do not wait on an operator setting.
 *
 * @param features A content list.
 * @returns How many are always shown.
 */
const ungated = (features: readonly Feature[]): number => features.filter(feature => !feature.requiresSetting).length

/**
 * Stands in for `usePerformanceStats` with the given figures.
 *
 * @param stats The figures the operator has filled in.
 */
const stubStats = (stats: PerformanceStat[]): void => {
  vi.stubGlobal('usePerformanceStats', () => computed(() => stats))
}

describe('NovaPerformance', () => {
  it('hides storage, cache and CDN claims until their settings exist', () => {
    stubTemplateGlobals()
    stubStats([])

    expect(mountWithKit(NovaPerformance).findAll('ul > li')).toHaveLength(ungated(PERFORMANCE_FEATURES))
  })

  it('shows a tile per figure the operator has set', () => {
    stubTemplateGlobals()
    stubStats([
      { key: PerformanceStatKey.Uptime, value: '99.9%' },
      { key: PerformanceStatKey.ResponseTime, value: '180 ms' },
    ])
    const wrapper = mountWithKit(NovaPerformance)

    expect(wrapper.find('[data-testid="nova-performance-stat-uptime"]').text()).toContain('99.9%')
    expect(wrapper.find('[data-testid="nova-performance-stat-response_time"]').text()).toContain('180 ms')
    expect(wrapper.find('[data-testid="nova-performance-stat-sites_hosted"]').exists()).toBe(false)
  })

  it('omits the figures grid when no figure is set', () => {
    stubTemplateGlobals()
    stubStats([])

    expect(mountWithKit(NovaPerformance).find('[data-testid="nova-performance-stats"]').exists()).toBe(false)
  })
})

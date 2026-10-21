/**
 * Unit tests for SectionsPerformance: one tile per published figure, in the order given, and no
 * section when no figure is set.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import SectionsPerformance from '~/components/sections/SectionsPerformance.vue'
import { PerformanceStatKey } from '~/constants/performanceStatKey'
import type { PerformanceStat } from '~/types/performancestat'
import { ACCENT_BRAND_CLASS } from '~/utils/ui/accent'

/** The figures the stubbed composable returns. */
const stats = ref<PerformanceStat[]>([])

vi.stubGlobal('usePerformanceStats', () => stats)

describe('SectionsPerformance', () => {
  beforeEach(() => {
    stats.value = [
      { key: PerformanceStatKey.Uptime, value: '99.9%' },
      { key: PerformanceStatKey.ResponseTime, value: '180 ms' },
    ]
  })

  it('shows each figure with its label', () => {
    const wrapper = mountWithKit(SectionsPerformance)

    expect(wrapper.find('[data-testid="performance-value-uptime"]').text()).toBe('99.9%')
    expect(wrapper.find('[data-testid="performance-response_time"]').text()).toContain('hostingSections.performance.stats.response_time')
  })

  it('shows only the figures that are set', () => {
    const wrapper = mountWithKit(SectionsPerformance)

    expect(wrapper.find('[data-testid="performance-sites_hosted"]').exists()).toBe(false)
  })

  it('uses the balanced grid', () => {
    const wrapper = mountWithKit(SectionsPerformance)

    expect(wrapper.find('[data-testid="sections-performance-grid"]').classes()).toContain('ui-balanced-grid')
  })

  it('renders nothing when no figure is set', () => {
    stats.value = []

    const wrapper = mountWithKit(SectionsPerformance)

    expect(wrapper.find('[data-testid="sections-performance"]').exists()).toBe(false)
  })

  it('points every tile at the brand colour so it is visible on a light page', () => {
    const wrapper = mountWithKit(SectionsPerformance)

    const tiles = wrapper.findAll('[class*="--accent:"]')
    expect(tiles.length).toBeGreaterThan(0)
    for (const tile of tiles) {
      expect(tile.classes()).toContain(ACCENT_BRAND_CLASS)
    }
  })
})

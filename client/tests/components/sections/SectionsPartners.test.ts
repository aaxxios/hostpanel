/**
 * Unit tests for SectionsPartners: the doubled marquee of clients and the four company figures.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import SectionsPartners from '~/components/sections/SectionsPartners.vue'
import { clients } from '~/lib/data'

describe('SectionsPartners', () => {
  it('shows the heading and its lead line', () => {
    const wrapper = mountWithKit(SectionsPartners)

    expect(wrapper.text()).toContain('clients.subtitle')
    expect(wrapper.text()).toContain('clients.heading')
  })

  it('lists every client twice, so the marquee loops without a gap', () => {
    const wrapper = mountWithKit(SectionsPartners)
    const first = clients[0]

    expect(wrapper.findAll(`[data-testid="partner-${first?.id}"]`)).toHaveLength(2)
    expect(wrapper.findAll(`[data-testid="partner-${first?.id}"]`)[0]?.text()).toContain(first?.name)
  })

  it('shows the four company figures', () => {
    const wrapper = mountWithKit(SectionsPartners)

    expect(wrapper.find('[data-testid="partner-stat-0"]').text()).toContain('50+')
    expect(wrapper.find('[data-testid="partner-stat-2"]').text()).toContain('$10M+')
    expect(wrapper.find('[data-testid="partner-stat-3"]').text()).toContain('99%')
  })

  it('pauses the marquee on hover', () => {
    const wrapper = mountWithKit(SectionsPartners)

    expect(wrapper.find('.animate-marquee').classes()).toContain('hover:[animation-play-state:paused]')
  })
})

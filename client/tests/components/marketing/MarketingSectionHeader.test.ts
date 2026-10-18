/**
 * Unit tests for MarketingSectionHeader: the badge, the plain and the rich title, and the lead line.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import MarketingSectionHeader from '~/components/marketing/MarketingSectionHeader.vue'

describe('MarketingSectionHeader', () => {
  it('renders the badge, the title and the subtitle', () => {
    const wrapper = mountWithKit(MarketingSectionHeader, { props: { badge: 'Tag', title: 'Heading', subtitle: 'Lead' } })

    expect(wrapper.find('[data-testid="section-header-badge"]').text()).toBe('Tag')
    expect(wrapper.find('[data-testid="section-header-title"]').element.tagName).toBe('H2')
    expect(wrapper.find('[data-testid="section-header-title"]').text()).toBe('Heading')
    expect(wrapper.find('[data-testid="section-header-subtitle"]').text()).toBe('Lead')
  })

  it('leaves out the badge and the subtitle when they are not given', () => {
    const wrapper = mountWithKit(MarketingSectionHeader, { props: { title: 'Heading' } })

    expect(wrapper.find('[data-testid="section-header-badge"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="section-header-subtitle"]').exists()).toBe(false)
  })

  it('prefers the title slot over the plain title', () => {
    const wrapper = mountWithKit(MarketingSectionHeader, { props: { title: 'Plain' }, slots: { title: 'Rich <b>word</b>' } })

    expect(wrapper.find('[data-testid="section-header-title"]').text()).toContain('Rich')
    expect(wrapper.find('[data-testid="section-header-title"]').text()).not.toContain('Plain')
  })
})

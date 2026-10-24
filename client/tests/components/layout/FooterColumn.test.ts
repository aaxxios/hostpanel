/**
 * Unit tests for LayoutFooterColumn: the heading, the link list and the per-link test ids.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import LayoutFooterColumn from '~/components/layout/FooterColumn.vue'

/** Props every test starts from. */
const props = {
  column: 'support',
  title: 'Support',
  links: [
    { key: 'faq', label: 'FAQ', to: '/en/faq' },
    { key: 'help', label: 'Help centre', to: '/en/knowledgebase' },
  ],
}

describe('LayoutFooterColumn', () => {
  it('renders the heading as an h3', () => {
    const wrapper = mountWithKit(LayoutFooterColumn, { props })

    expect(wrapper.find('h3').text()).toBe('Support')
  })

  it('renders every link with its route and a column scoped test id', () => {
    const wrapper = mountWithKit(LayoutFooterColumn, { props })

    expect(wrapper.find('[data-testid="footer-support-faq"]').attributes('href')).toBe('/en/faq')
    expect(wrapper.find('[data-testid="footer-support-help"]').text()).toBe('Help centre')
  })

  it('colours the links from the classic tokens', () => {
    const link = mountWithKit(LayoutFooterColumn, { props }).find('[data-testid="footer-support-faq"]')

    expect(link.classes()).toEqual(expect.arrayContaining(['text-classic-muted', 'hover:text-classic-brand']))
  })

  it('exposes the column as a test hook', () => {
    expect(mountWithKit(LayoutFooterColumn, { props }).find('[data-testid="footer-column-support"]').exists()).toBe(true)
  })
})

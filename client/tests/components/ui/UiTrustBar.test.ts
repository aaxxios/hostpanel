/**
 * Unit tests for UiTrustBar: centred items with chips, an optional bold figure and no shadow.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import UiTrustBar from '~/components/ui/UiTrustBar.vue'
import { UiTone } from '~/utils/ui/enums'

/** Sample items. */
const ITEMS = [
  { key: 'a', icon: 'lucide:lock', label: 'Secure', tone: UiTone.Success },
  { key: 'b', icon: 'lucide:activity', label: 'uptime', value: '99.9%' },
]

describe('UiTrustBar', () => {
  it('renders an item and a chip per entry', () => {
    const wrapper = mountWithKit(UiTrustBar, { props: { items: ITEMS } })

    expect(wrapper.findAll('[data-testid="trust-item"]')).toHaveLength(2)
    expect(wrapper.findAll('[data-testid="trust-chip"]')).toHaveLength(2)
  })

  it('prints the figure in bold before the label', () => {
    const wrapper = mountWithKit(UiTrustBar, { props: { items: ITEMS } })

    expect(wrapper.find('strong').text()).toBe('99.9%')
  })

  it('has no drop shadow, so nothing shows under the bottom edge', () => {
    const classes = mountWithKit(UiTrustBar, { props: { items: ITEMS } }).classes()

    expect(classes.some(name => name.startsWith('shadow'))).toBe(false)
  })

  it('lets the screen replace the surface', () => {
    const wrapper = mountWithKit(UiTrustBar, { props: { items: ITEMS, surfaceClass: 'bg-surf' } })

    expect(wrapper.classes()).toContain('bg-surf')
    expect(wrapper.classes()).not.toContain('bg-ui-surface')
  })

  it('balances six items into three columns instead of five plus one', () => {
    const six = Array.from({ length: 6 }, (_, index) => ({ key: `k${index}`, icon: 'lucide:lock', label: `L${index}` }))
    const list = mountWithKit(UiTrustBar, { props: { items: six } }).find('ul')

    expect(list.classes()).toContain('lg:grid-cols-[repeat(3,max-content)]')
    expect(list.classes()).toContain('justify-center')
  })

  it('keeps three items in one row', () => {
    const three = [...ITEMS, { key: 'c', icon: 'lucide:lock', label: 'C' }]

    expect(mountWithKit(UiTrustBar, { props: { items: three } }).find('ul').classes()).toContain('sm:grid-cols-[repeat(3,max-content)]')
  })
})

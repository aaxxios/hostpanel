/**
 * Unit tests for UiStatGroup: one equal column per stat and dividers between cells only.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import UiStatGroup from '~/components/ui/UiStatGroup.vue'

/** Three sample stats. */
const STATS = [
  { value: '500+', label: 'Clients' },
  { value: '99.9%', label: 'Uptime' },
  { value: 'Europe, North America', label: 'Regions' },
]

describe('UiStatGroup', () => {
  it('renders every stat with the same component', () => {
    const wrapper = mountWithKit(UiStatGroup, { props: { stats: STATS } })

    expect(wrapper.findAll('[data-testid="ui-stat-group-item"]')).toHaveLength(3)
    expect(wrapper.text()).toContain('Europe, North America')
  })

  it('uses two columns on a phone and one per stat from sm up', () => {
    const classes = mountWithKit(UiStatGroup, { props: { stats: STATS } }).classes()

    expect(classes).toEqual(expect.arrayContaining(['grid-cols-2', 'sm:grid-cols-3']))
  })

  it('caps the columns at four', () => {
    const many = Array.from({ length: 6 }, (_, index) => ({ value: String(index), label: 'x' }))

    expect(mountWithKit(UiStatGroup, { props: { stats: many } }).classes()).toContain('sm:grid-cols-4')
  })

  it('draws no divider before the first cell and passes the divider colour', () => {
    const items = mountWithKit(UiStatGroup, { props: { stats: STATS, dividerClass: 'border-line' } }).findAll('[data-testid="ui-stat-group-item"]')

    expect(items.at(0)?.classes()).toContain('sm:first:border-l-0')
    expect(items.at(1)?.classes()).toContain('border-line')
  })
})

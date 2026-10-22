/**
 * Unit tests for UiStat: value and caption, the shared sizes and the full text in the title.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import UiStat from '~/components/ui/UiStat.vue'

describe('UiStat', () => {
  it('renders the value and the label', () => {
    const wrapper = mountWithKit(UiStat, { props: { value: '99.9%', label: 'Uptime' } })

    expect(wrapper.find('[data-testid="ui-stat-value"]').text()).toBe('99.9%')
    expect(wrapper.find('[data-testid="ui-stat-label"]').text()).toBe('Uptime')
  })

  it('keeps the full text in the title and clamps long values', () => {
    const wrapper = mountWithKit(UiStat, { props: { value: 'Europe, North America', label: 'Regions' } })
    const value = wrapper.find('[data-testid="ui-stat-value"]')

    expect(value.attributes('title')).toBe('Europe, North America')
    expect(value.classes()).toContain('line-clamp-2')
  })

  it('bottom-aligns the cell so captions in a row line up when one value wraps', () => {
    const wrapper = mountWithKit(UiStat, { props: { value: 'Europe, North America', label: 'Regions' } })

    expect(wrapper.classes()).toEqual(expect.arrayContaining(['flex', 'flex-col', 'justify-end']))
  })

  it('applies the colour classes the screen passes', () => {
    const wrapper = mountWithKit(UiStat, { props: { value: 5, label: 'x', valueClass: 'text-tx', labelClass: 'text-mut' } })

    expect(wrapper.find('[data-testid="ui-stat-value"]').classes()).toContain('text-tx')
    expect(wrapper.find('[data-testid="ui-stat-label"]').classes()).toContain('text-mut')
  })
})

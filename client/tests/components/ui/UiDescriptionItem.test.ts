/**
 * Unit tests for DescriptionItem: the term, the value, the em dash for empty values, the slot
 * override and the value class.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import DescriptionItem from '~/components/ui/UiDescriptionItem.vue'

describe('DescriptionItem', () => {
  it('renders the label as a term and the value as its description', () => {
    const wrapper = mountWithKit(DescriptionItem, { props: { label: 'Status', value: 'Active' } })

    expect(wrapper.find('dt').text()).toBe('Status')
    expect(wrapper.find('dd').text()).toBe('Active')
  })

  it('renders a numeric value', () => {
    const wrapper = mountWithKit(DescriptionItem, { props: { label: 'Seats', value: 0 } })

    expect(wrapper.find('dd').text()).toBe('0')
  })

  it.each([[undefined], [null]])('shows an em dash for %s', (value) => {
    const wrapper = mountWithKit(DescriptionItem, { props: { label: 'Status', value } })

    expect(wrapper.find('dd').text()).toBe('\u2014')
  })

  it('lets the slot replace the value', () => {
    const wrapper = mountWithKit(DescriptionItem, { props: { label: 'Status', value: 'Ignored' }, slots: { default: 'Custom' } })

    expect(wrapper.find('dd').text()).toBe('Custom')
  })

  it('adds the value class to the description', () => {
    const wrapper = mountWithKit(DescriptionItem, { props: { label: 'Status', value: 'x', valueClass: 'font-mono' } })

    expect(wrapper.find('dd').classes()).toContain('font-mono')
  })
})

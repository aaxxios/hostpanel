/**
 * Unit tests for UiTable: the framed shell around a real table and the scroll option.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import UiTable from '~/components/ui/UiTable.vue'

describe('UiTable', () => {
  it('frames a table that holds the slot', () => {
    const wrapper = mountWithKit(UiTable, { slots: { default: '<tbody><tr><td>cell</td></tr></tbody>' } })

    expect(wrapper.classes()).toEqual(expect.arrayContaining(['rounded-2xl', 'border', 'overflow-hidden']))
    expect(wrapper.find('table td').text()).toBe('cell')
  })

  it('scrolls sideways only when scrollable', () => {
    const scrollable = mountWithKit(UiTable, { props: { scrollable: true } })
    const fixed = mountWithKit(UiTable)

    expect(scrollable.find('.overflow-x-auto').exists()).toBe(true)
    expect(fixed.find('.overflow-x-auto').exists()).toBe(false)
  })

  it('passes data-testid to the frame', () => {
    const wrapper = mountWithKit(UiTable, { attrs: { 'data-testid': 'invoices-table' } })

    expect(wrapper.attributes('data-testid')).toBe('invoices-table')
  })

  it('makes a labelled scroll area a focus stop', () => {
    const scroller = mountWithKit(UiTable, { props: { scrollable: true, label: 'Plans' } }).find('.overflow-x-auto')

    expect(scroller.attributes('tabindex')).toBe('0')
    expect(scroller.attributes('role')).toBe('region')
    expect(scroller.attributes('aria-label')).toBe('Plans')
  })

  it('leaves the scroll area out of the tab order without a label', () => {
    const scroller = mountWithKit(UiTable, { props: { scrollable: true } }).find('.overflow-x-auto')

    expect(scroller.attributes('tabindex')).toBeUndefined()
  })
})

/**
 * Tests for ReportField: a caption above its control, sized by the width choice.
 */
import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import ReportField from '@/components/reports/ReportField.vue'

describe('ReportField', () => {
  it('shows the label above the slot content', () => {
    const wrapper = mount(ReportField, { props: { label: 'Year' }, slots: { default: '<i data-testid="control" />' } })

    expect(wrapper.get('label').text()).toBe('Year')
    expect(wrapper.find('[data-testid="control"]').exists()).toBe(true)
  })

  it('takes the free space of the row when it grows', () => {
    const wrapper = mount(ReportField, { props: { label: 'Dates', width: 'grow' } })

    expect(wrapper.classes()).toContain('flex-1')
  })

  it('has a fixed width otherwise', () => {
    expect(mount(ReportField, { props: { label: 'Year', width: 'sm' } }).classes()).toContain('w-28')
  })
})

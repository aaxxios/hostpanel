/**
 * Unit tests for UiCheckbox: checkbox semantics, toggling, change event and disabled state.
 */
import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import UiCheckbox from '@/components/ui/UiCheckbox.vue'

describe('UiCheckbox', () => {
  it('is a checkbox that reports its state', () => {
    const wrapper = mount(UiCheckbox, { props: { modelValue: true } })

    expect(wrapper.attributes('role')).toBe('checkbox')
    expect(wrapper.attributes('aria-checked')).toBe('true')
    expect(wrapper.find('svg').exists()).toBe(true)
  })

  it('shows no tick when unchecked', () => {
    expect(mount(UiCheckbox, { props: { modelValue: false } }).find('svg').exists()).toBe(false)
  })

  it('emits the flipped state and a change event on click', async () => {
    const wrapper = mount(UiCheckbox, { props: { modelValue: false } })

    await wrapper.trigger('click')

    expect(wrapper.emitted('update:modelValue')?.[0]).toEqual([true])
    expect(wrapper.emitted('change')).toHaveLength(1)
  })

  it('does nothing while disabled', async () => {
    const wrapper = mount(UiCheckbox, { props: { modelValue: false, disabled: true } })

    await wrapper.trigger('click')

    expect(wrapper.emitted('update:modelValue')).toBeUndefined()
  })

  it('keeps data-testid on the button', () => {
    const wrapper = mount(UiCheckbox, { props: { modelValue: false }, attrs: { 'data-testid': 'probe' } })

    expect(wrapper.attributes('data-testid')).toBe('probe')
  })
})

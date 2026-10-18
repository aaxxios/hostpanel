/**
 * Unit tests for UiNativeSelect: slot options, model updates, placeholder option, themed field.
 */
import { describe, expect, it } from 'vitest'
import { h } from 'vue'
import { mount } from '@vue/test-utils'
import UiNativeSelect from '@/components/ui/UiNativeSelect.vue'

/** Two options passed through the default slot. */
const options = () => [
  h('option', { value: 'a' }, 'A'),
  h('option', { value: 'b' }, 'B'),
]

describe('UiNativeSelect', () => {
  it('renders slot options and selects the model value', () => {
    const wrapper = mount(UiNativeSelect, { props: { modelValue: 'b' }, slots: { default: options } })

    expect(wrapper.element.tagName).toBe('SELECT')
    expect((wrapper.element as HTMLSelectElement).value).toBe('b')
  })

  it('emits the chosen value', async () => {
    const wrapper = mount(UiNativeSelect, { slots: { default: options } })

    await wrapper.setValue('b')

    expect(wrapper.emitted('update:modelValue')?.[0]).toEqual(['b'])
  })

  it('passes classes through and draws the themed field', () => {
    const wrapper = mount(UiNativeSelect, { attrs: { class: 'w-40' } })

    expect(wrapper.classes()).toEqual(expect.arrayContaining(['w-40', 'bg-surface-field']))
  })

  it('renders a disabled placeholder option first', () => {
    const wrapper = mount(UiNativeSelect, { props: { placeholder: 'Choose' }, slots: { default: options } })
    const first = wrapper.get('option')

    expect(first.text()).toBe('Choose')
    expect(first.attributes('disabled')).toBeDefined()
  })
})

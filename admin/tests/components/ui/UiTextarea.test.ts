/**
 * Unit tests for UiTextarea: model updates, themed field, invalid state.
 */
import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import UiTextarea from '@/components/ui/UiTextarea.vue'

describe('UiTextarea', () => {
  it('renders a textarea showing the model value', () => {
    const wrapper = mount(UiTextarea, { props: { modelValue: 'line' }, attrs: { rows: '3' } })

    expect(wrapper.element.tagName).toBe('TEXTAREA')
    expect((wrapper.element as HTMLTextAreaElement).value).toBe('line')
    expect(wrapper.attributes('rows')).toBe('3')
  })

  it('emits the typed text', async () => {
    const wrapper = mount(UiTextarea)

    await wrapper.setValue('typed')

    expect(wrapper.emitted('update:modelValue')?.[0]).toEqual(['typed'])
  })

  it('shows the placeholder and the themed field classes', () => {
    const wrapper = mount(UiTextarea, { props: { placeholder: 'Write here' } })

    expect(wrapper.attributes('placeholder')).toBe('Write here')
    expect(wrapper.classes()).toEqual(expect.arrayContaining(['bg-surface-field', 'resize-y']))
  })

  it('marks an invalid field', () => {
    const wrapper = mount(UiTextarea, { props: { invalid: true } })

    expect(wrapper.attributes('aria-invalid')).toBe('true')
  })
})

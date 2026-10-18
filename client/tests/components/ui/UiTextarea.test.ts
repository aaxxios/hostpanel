/**
 * Unit tests for UiTextarea: label binding, v-model, the character counter and the error state.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import UiTextarea from '~/components/ui/UiTextarea.vue'

describe('UiTextarea', () => {
  it('binds the label to the textarea', () => {
    const wrapper = mountWithKit(UiTextarea, { props: { label: 'Message', placeholder: 'Write' } })

    expect(wrapper.find('label').attributes('for')).toBe(wrapper.find('textarea').attributes('id'))
    expect(wrapper.find('label').text()).toContain('Message')
  })

  it('emits the typed value', async () => {
    const wrapper = mountWithKit(UiTextarea, { props: { placeholder: 'Write' } })

    await wrapper.find('textarea').setValue('hello there')

    expect(wrapper.emitted('update:modelValue')?.[0]).toEqual(['hello there'])
  })

  it('passes rows, placeholder and testid', () => {
    const wrapper = mountWithKit(UiTextarea, { props: { rows: 7, placeholder: 'Write', testid: 'msg' } })

    const area = wrapper.find('textarea')
    expect(area.attributes('rows')).toBe('7')
    expect(area.attributes('placeholder')).toBe('Write')
    expect(area.attributes('data-testid')).toBe('msg')
  })

  it('shows the counter only with a max length', () => {
    const counted = mountWithKit(UiTextarea, { props: { modelValue: 'abc', maxLength: 10, placeholder: 'x' } })
    const free = mountWithKit(UiTextarea, { props: { modelValue: 'abc', placeholder: 'x' } })

    expect(counted.text()).toContain('3/10')
    expect(counted.find('textarea').attributes('maxlength')).toBe('10')
    expect(free.text()).not.toContain('/')
  })

  it('shows the error instead of the hint', () => {
    const wrapper = mountWithKit(UiTextarea, { props: { error: 'Too short', hint: 'Help', placeholder: 'x' } })

    expect(wrapper.text()).toContain('Too short')
    expect(wrapper.text()).not.toContain('Help')
    expect(wrapper.find('textarea').attributes('aria-invalid')).toBe('true')
  })

  it('toggles vertical resize', () => {
    expect(mountWithKit(UiTextarea, { props: { placeholder: 'x' } }).find('textarea').classes()).toContain('resize-y')
    expect(mountWithKit(UiTextarea, { props: { resize: false, placeholder: 'x' } }).find('textarea').classes()).toContain('resize-none')
  })
})

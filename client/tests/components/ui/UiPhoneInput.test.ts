/**
 * Unit tests for PhoneInput with the third-party tel widget replaced by a stand-in that exposes
 * the options it was given and can emit its input event: the label, the error and hint lines,
 * the options handed to the widget and the value normalisation.
 */
import { describe, expect, it, vi } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import PhoneInput from '~/components/ui/UiPhoneInput.vue'

vi.mock('vue-tel-input', async () => {
  const { defineComponent, h } = await import('vue')
  return {
    VueTelInput: defineComponent({
      name: 'VueTelInput',
      props: ['modelValue', 'mode', 'disabled', 'defaultCountry', 'inputOptions', 'required'],
      emits: ['on-input'],
      setup: () => () => h('div', { 'data-testid': 'tel-widget' }),
    }),
  }
})

describe('PhoneInput', () => {
  it('renders the label with the required asterisk', () => {
    const wrapper = mountWithKit(PhoneInput, { props: { label: 'Phone', required: true } })

    expect(wrapper.find('label').text()).toContain('Phone')
    expect(wrapper.find('label span').text()).toBe('*')
  })

  it('hands the widget its options', () => {
    const wrapper = mountWithKit(PhoneInput, { props: { id: 'phone', modelValue: '+37410123456', disabled: true } })

    const widget = wrapper.findComponent({ name: 'VueTelInput' })
    expect(widget.props('modelValue')).toBe('+37410123456')
    expect(widget.props('mode')).toBe('international')
    expect(widget.props('disabled')).toBe(true)
    expect(widget.props('defaultCountry')).toBe('AM')
    expect(widget.props('inputOptions')).toMatchObject({ id: 'phone', placeholder: 'ui.phoneExample' })
  })

  it('emits the parsed international number when there is one', () => {
    const wrapper = mountWithKit(PhoneInput)

    wrapper.findComponent({ name: 'VueTelInput' }).vm.$emit('on-input', '10 123456', { number: '+37410123456' })

    expect(wrapper.emitted('update:modelValue')?.[0]).toEqual(['+37410123456'])
    expect(wrapper.emitted('onInput')?.[0]).toEqual([{ number: '+37410123456' }])
  })

  it('falls back to the raw text when the number does not parse', () => {
    const wrapper = mountWithKit(PhoneInput)

    wrapper.findComponent({ name: 'VueTelInput' }).vm.$emit('on-input', '10 12', {})

    expect(wrapper.emitted('update:modelValue')?.[0]).toEqual(['10 12'])
  })

  it('shows the error, marks the container and hides the hint', () => {
    const wrapper = mountWithKit(PhoneInput, { props: { error: 'Invalid number', hint: 'Include the country code' } })

    expect(wrapper.text()).toContain('Invalid number')
    expect(wrapper.text()).not.toContain('Include the country code')
    expect(wrapper.find('.phone-input-container').classes()).toContain('has-error')
  })

  it('shows the hint when there is no error and passes the testid', () => {
    const wrapper = mountWithKit(PhoneInput, { props: { hint: 'Include the country code', testid: 'phone-field' } })

    expect(wrapper.text()).toContain('Include the country code')
    expect(wrapper.find('.phone-input-container').attributes('data-testid')).toBe('phone-field')
  })
})

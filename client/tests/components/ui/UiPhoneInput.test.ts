/**
 * Unit tests for PhoneInput: the label, the error and hint lines, the stored value format
 * (international when valid, as typed otherwise), the country change, and the way a value that
 * arrives from outside is split back into country and number.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import PhoneInput from '~/components/ui/UiPhoneInput.vue'

describe('PhoneInput', () => {
  it('renders the label with the required asterisk, bound to the number input', () => {
    const wrapper = mountWithKit(PhoneInput, { props: { id: 'phone', label: 'Phone', required: true } })

    expect(wrapper.find('label').text()).toContain('Phone')
    expect(wrapper.find('label span').text()).toBe('*')
    expect(wrapper.find('label').attributes('for')).toBe('phone')
    expect(wrapper.find('input#phone').attributes('placeholder')).toBe('ui.phoneExample')
  })

  it('starts on Armenia and shows its calling code', () => {
    const wrapper = mountWithKit(PhoneInput)

    expect(wrapper.get('[data-testid="phone-country-trigger"]').text()).toContain('+374')
  })

  it('emits the number in international format when it is valid', async () => {
    const wrapper = mountWithKit(PhoneInput, { props: { testid: 'phone' } })

    await wrapper.get('[data-testid="phone"]').setValue('77 123456')

    expect(wrapper.emitted('update:modelValue')?.[0]).toEqual(['+37477123456'])
  })

  it('emits the text behind the calling code while the number is not valid yet', async () => {
    const wrapper = mountWithKit(PhoneInput, { props: { testid: 'phone' } })

    await wrapper.get('[data-testid="phone"]').setValue('77 12')

    expect(wrapper.emitted('update:modelValue')?.[0]).toEqual(['+374 77 12'])
  })

  it('emits an empty value when the number is cleared', async () => {
    const wrapper = mountWithKit(PhoneInput, { props: { testid: 'phone' } })
    await wrapper.get('[data-testid="phone"]').setValue('77 123456')

    await wrapper.get('[data-testid="phone"]').setValue('')

    expect(wrapper.emitted('update:modelValue')?.at(-1)).toEqual([''])
  })

  it('re-emits the value for the new country when the country changes', async () => {
    const wrapper = mountWithKit(PhoneInput, { props: { testid: 'phone' }, attachToBody: true })
    await wrapper.get('[data-testid="phone"]').setValue('2079460958')

    await wrapper.get('[data-testid="phone-country-trigger"]').trigger('click')
    await wrapper.get('[data-testid="phone-country-GB"]').trigger('click')

    expect(wrapper.emitted('update:modelValue')?.at(-1)).toEqual(['+442079460958'])
    expect(wrapper.get('[data-testid="phone-country-trigger"]').text()).toContain('+44')
    wrapper.unmount()
  })

  it('splits a value from outside into the country and the national number', () => {
    const wrapper = mountWithKit(PhoneInput, { props: { modelValue: '+442079460958', testid: 'phone' } })

    expect(wrapper.get('[data-testid="phone-country-trigger"]').text()).toContain('+44')
    expect((wrapper.get('[data-testid="phone"]').element as HTMLInputElement).value).toBe('2079460958')
  })

  it('keeps the chosen country while the parent echoes the emitted value back', async () => {
    const wrapper = mountWithKit(PhoneInput, { props: { testid: 'phone' } })

    await wrapper.get('[data-testid="phone"]').setValue('77 123456')
    await wrapper.setProps({ modelValue: '+37477123456' })

    expect(wrapper.get('[data-testid="phone-country-trigger"]').text()).toContain('+374')
    expect((wrapper.get('[data-testid="phone"]').element as HTMLInputElement).value).toBe('77 123456')
  })

  it('clears the number when the parent resets the value', async () => {
    const wrapper = mountWithKit(PhoneInput, { props: { modelValue: '+37477123456', testid: 'phone' } })

    await wrapper.setProps({ modelValue: '' })

    expect((wrapper.get('[data-testid="phone"]').element as HTMLInputElement).value).toBe('')
  })

  it('disables both the picker and the input', () => {
    const wrapper = mountWithKit(PhoneInput, { props: { disabled: true, testid: 'phone' } })

    expect(wrapper.get('[data-testid="phone"]').attributes('disabled')).toBeDefined()
    expect(wrapper.get('[data-testid="phone-country-trigger"]').attributes('disabled')).toBeDefined()
  })

  it('shows the error, marks the input invalid and hides the hint', () => {
    const wrapper = mountWithKit(PhoneInput, { props: { error: 'Invalid number', hint: 'Include the country code', testid: 'phone' } })

    expect(wrapper.text()).toContain('Invalid number')
    expect(wrapper.text()).not.toContain('Include the country code')
    expect(wrapper.get('[data-testid="phone"]').attributes('aria-invalid')).toBe('true')
  })

  it('shows the hint when there is no error', () => {
    const wrapper = mountWithKit(PhoneInput, { props: { hint: 'Include the country code' } })

    expect(wrapper.text()).toContain('Include the country code')
  })
})

/**
 * Unit tests for UiPhoneInput: country code trigger, country list, typed number and the
 * per-country hint. The component carries no stylesheet of its own.
 */
import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import UiPhoneInput from '@/components/ui/UiPhoneInput.vue'

describe('UiPhoneInput', () => {
  it('defaults to the first listed country', () => {
    const wrapper = mount(UiPhoneInput)

    expect(wrapper.get('[data-testid="phone-country-trigger"]').text()).toContain('+1')
    expect(wrapper.get('[data-testid="phone-number-input"]').attributes('placeholder')).toBe('201-555-0123')
  })

  it('shows the code and sample number of the chosen country', () => {
    const wrapper = mount(UiPhoneInput, { props: { countryCode: 'GB' } })

    expect(wrapper.get('[data-testid="phone-country-trigger"]').text()).toContain('+44')
    expect(wrapper.get('[data-testid="phone-number-input"]').attributes('placeholder')).toBe('7700 900000')
  })

  it('shows the number it was given and emits typing', async () => {
    const wrapper = mount(UiPhoneInput, { props: { phoneNumber: '123' } })
    const input = wrapper.get('[data-testid="phone-number-input"]')

    expect((input.element as HTMLInputElement).value).toBe('123')

    await input.setValue('456')

    expect(wrapper.emitted('update:phoneNumber')?.[0]).toEqual(['456'])
  })

  it('opens the list, emits the picked country and closes', async () => {
    const wrapper = mount(UiPhoneInput)

    await wrapper.get('[data-testid="phone-country-trigger"]').trigger('click')
    expect(wrapper.find('[data-testid="phone-country-GB"]').exists()).toBe(true)

    await wrapper.get('[data-testid="phone-country-GB"]').trigger('click')

    expect(wrapper.emitted('update:countryCode')?.[0]).toEqual(['GB'])
    expect(wrapper.find('[data-testid="phone-country-GB"]').exists()).toBe(false)
  })

  it('draws each flag as text from the country list, not as an image from a host', async () => {
    const wrapper = mount(UiPhoneInput)

    await wrapper.get('[data-testid="phone-country-trigger"]').trigger('click')

    expect(wrapper.html()).not.toContain('style=')
    expect(wrapper.html()).not.toContain('http')
    expect(wrapper.get('[data-testid="phone-country-AM"]').text()).toContain('+374')
  })
})

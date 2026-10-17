/**
 * Unit tests for UiInput: label binding, v-model, error and hint lines, the password reveal and
 * the icon/prefix padding.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import UiInput from '~/components/ui/UiInput.vue'
import { UiInputType } from '~/utils/ui/enums'

describe('UiInput', () => {
  it('binds the label to the input', () => {
    const wrapper = mountWithKit(UiInput, { props: { label: 'Email', placeholder: 'you@example.com' } })

    const label = wrapper.find('label')
    const input = wrapper.find('input')
    expect(label.text()).toContain('Email')
    expect(label.attributes('for')).toBe(input.attributes('id'))
    expect(input.attributes('id')).toBeTruthy()
  })

  it('uses the id it is given', () => {
    const wrapper = mountWithKit(UiInput, { props: { id: 'email-field', label: 'Email', placeholder: 'x' } })

    expect(wrapper.find('input').attributes('id')).toBe('email-field')
    expect(wrapper.find('label').attributes('for')).toBe('email-field')
  })

  it('shows the required asterisk', () => {
    const wrapper = mountWithKit(UiInput, { props: { label: 'Name', required: true, placeholder: 'x' } })

    expect(wrapper.find('label span').text()).toBe('*')
    expect(wrapper.find('input').attributes('required')).toBeDefined()
  })

  it('emits the typed value', async () => {
    const wrapper = mountWithKit(UiInput, { props: { placeholder: 'x' } })

    await wrapper.find('input').setValue('hello')

    expect(wrapper.emitted('update:modelValue')?.[0]).toEqual(['hello'])
  })

  it('emits focus and blur', async () => {
    const wrapper = mountWithKit(UiInput, { props: { placeholder: 'x' } })

    await wrapper.find('input').trigger('focus')
    await wrapper.find('input').trigger('blur')

    expect(wrapper.emitted('focus')).toHaveLength(1)
    expect(wrapper.emitted('blur')).toHaveLength(1)
  })

  it('passes the placeholder and the testid to the input', () => {
    const wrapper = mountWithKit(UiInput, { props: { placeholder: 'Type here', testid: 'name-input' } })

    const input = wrapper.find('input')
    expect(input.attributes('placeholder')).toBe('Type here')
    expect(input.attributes('data-testid')).toBe('name-input')
  })

  it('shows the error instead of the hint and marks the input invalid', () => {
    const wrapper = mountWithKit(UiInput, { props: { error: 'Required', hint: 'Help', placeholder: 'x' } })

    expect(wrapper.text()).toContain('Required')
    expect(wrapper.text()).not.toContain('Help')
    expect(wrapper.find('input').attributes('aria-invalid')).toBe('true')
    expect(wrapper.find('input').classes()).toContain('border-red-400')
  })

  it('shows the hint when there is no error', () => {
    const wrapper = mountWithKit(UiInput, { props: { hint: 'Help', placeholder: 'x' } })

    expect(wrapper.text()).toContain('Help')
    expect(wrapper.find('input').attributes('aria-invalid')).toBeUndefined()
  })

  it('reveals and hides a password', async () => {
    const wrapper = mountWithKit(UiInput, { props: { type: UiInputType.Password, placeholder: 'x' } })
    expect(wrapper.find('input').attributes('type')).toBe('password')

    await wrapper.find('button').trigger('click')
    expect(wrapper.find('input').attributes('type')).toBe('text')

    await wrapper.find('button').trigger('click')
    expect(wrapper.find('input').attributes('type')).toBe('password')
  })

  it('pads for an icon or a prefix slot', () => {
    const withIcon = mountWithKit(UiInput, { props: { icon: 'lucide:search', placeholder: 'x' } })
    const withPrefix = mountWithKit(UiInput, { props: { placeholder: 'x' }, slots: { prefix: '@' } })
    const plain = mountWithKit(UiInput, { props: { placeholder: 'x' } })

    expect(withIcon.find('input').classes()).toContain('pl-10')
    expect(withPrefix.find('input').classes()).toContain('pl-10')
    expect(plain.find('input').classes()).not.toContain('pl-10')
  })

  it('disables the input', () => {
    const wrapper = mountWithKit(UiInput, { props: { disabled: true, placeholder: 'x' } })

    expect(wrapper.find('input').attributes('disabled')).toBeDefined()
    expect(wrapper.find('input').classes()).toContain('cursor-not-allowed')
  })

  it('puts the form field name on the input, not on the wrapper', () => {
    const wrapper = mountWithKit(UiInput, { props: { name: 'email', placeholder: 'you@example.com' } })

    expect(wrapper.find('input').attributes('name')).toBe('email')
  })

  it('tags the password reveal button from the input testid', () => {
    const named = mountWithKit(UiInput, { props: { type: UiInputType.Password, testid: 'login-password', placeholder: 'x' } })
    const plain = mountWithKit(UiInput, { props: { type: UiInputType.Password, placeholder: 'x' } })

    expect(named.find('button').attributes('data-testid')).toBe('login-password-toggle')
    expect(plain.find('button').attributes('data-testid')).toBe('ui-input-toggle')
  })
})

/**
 * Mount tests for SecretSettingField: a stored secret shows masked with Replace and Clear, Replace
 * opens an empty password input, Cancel drops what was typed, Clear asks the parent, and an unset
 * secret is an empty password input from the start.
 */
import { describe, expect, it } from 'vitest'
import SecretSettingField from '@/components/settings/SecretSettingField.vue'
import { mountWithApp } from '../../helpers/mount'

/**
 * Mounts the control.
 *
 * @param props - Props to change from a stored, untouched secret.
 * @returns The wrapper.
 */
const mountField = (props: Record<string, unknown> = {}) =>
  mountWithApp(SecretSettingField, { props: { hasValue: true, modelValue: '', testid: 'secret', ...props } })

describe('SecretSettingField', () => {
  it('masks a stored secret and offers Replace and Clear, with no input', () => {
    const wrapper = mountField()

    expect(wrapper.get('[data-testid="secret-masked"]').text()).toBe('••••••, set')
    expect(wrapper.find('[data-testid="secret-input"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="secret-replace"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="secret-clear"]').exists()).toBe(true)
  })

  it('opens an empty password input on Replace and emits what is typed', async () => {
    const wrapper = mountField()

    await wrapper.get('[data-testid="secret-replace"]').trigger('click')
    const input = wrapper.get('[data-testid="secret-input"]')
    expect(input.attributes('type')).toBe('password')
    expect((input.element as HTMLInputElement).value).toBe('')
    await input.setValue('new-secret')

    expect(wrapper.emitted('update:modelValue')?.at(-1)).toEqual(['new-secret'])
  })

  it('drops the typed value and shows the mask again on Cancel', async () => {
    const wrapper = mountField()

    await wrapper.get('[data-testid="secret-replace"]').trigger('click')
    await wrapper.get('[data-testid="secret-cancel"]').trigger('click')

    expect(wrapper.emitted('update:modelValue')?.at(-1)).toEqual([''])
    expect(wrapper.find('[data-testid="secret-masked"]').exists()).toBe(true)
  })

  it('emits clear from the Clear button', async () => {
    const wrapper = mountField()

    await wrapper.get('[data-testid="secret-clear"]').trigger('click')

    expect(wrapper.emitted('clear')).toHaveLength(1)
  })

  it('shows an empty password input and no Cancel when nothing is stored', () => {
    const wrapper = mountField({ hasValue: false })

    const input = wrapper.get('[data-testid="secret-input"]')
    expect(input.attributes('type')).toBe('password')
    expect(input.attributes('placeholder')).toBe('Not set')
    expect(wrapper.find('[data-testid="secret-cancel"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="secret-masked"]').exists()).toBe(false)
  })
})

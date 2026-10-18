/**
 * Unit tests for ProfileEmailPrefs: the six categories, read-only mode and the emitted flags.
 */
import { describe, expect, it } from 'vitest'
import { mountAccount } from '~/tests/helpers/mountAccount'
import ProfileEmailPrefs from '~/components/account/ProfileEmailPrefs.vue'
import { decodeEmailPrefs } from '~/utils/emailPreferences'

describe('ProfileEmailPrefs', () => {
  it('renders one checkbox per category', () => {
    const wrapper = mountAccount(ProfileEmailPrefs, { props: { modelValue: decodeEmailPrefs(undefined) } })

    expect(wrapper.findAll('[role="checkbox"]')).toHaveLength(6)
  })

  it('shows the state of each flag', () => {
    const flags = { ...decodeEmailPrefs(undefined), invoice: false }
    const wrapper = mountAccount(ProfileEmailPrefs, { props: { modelValue: flags } })

    expect(wrapper.find('[data-testid="account-pref-invoice"]').attributes('aria-checked')).toBe('false')
    expect(wrapper.find('[data-testid="account-pref-general"]').attributes('aria-checked')).toBe('true')
  })

  it('emits the full flag set with the clicked category flipped', async () => {
    const wrapper = mountAccount(ProfileEmailPrefs, { props: { modelValue: decodeEmailPrefs(undefined) } })

    await wrapper.find('[data-testid="account-pref-domain"]').trigger('click')

    const emitted = wrapper.emitted('update:modelValue')
    expect(emitted?.[0]?.[0]).toMatchObject({ domain: false, general: true })
  })

  it('does not emit when disabled', async () => {
    const wrapper = mountAccount(ProfileEmailPrefs, { props: { modelValue: decodeEmailPrefs(undefined), disabled: true } })

    await wrapper.find('[data-testid="account-pref-domain"]').trigger('click')

    expect(wrapper.emitted('update:modelValue')).toBeUndefined()
  })
})

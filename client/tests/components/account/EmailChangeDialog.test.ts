/**
 * Unit tests for EmailChangeDialog: the two fields, the local checks and the request through the
 * account store.
 */
import { describe, expect, it, vi } from 'vitest'
import { mountAccount } from '~/tests/helpers/mountAccount'
import EmailChangeDialog from '~/components/account/EmailChangeDialog.vue'
import { useAccountStore } from '~/stores/account'
import { ActionOutcome } from '~/types/actionoutcome'

vi.mock('~/composables/apis/useClientApi', () => ({ useClientApi: () => ({}) }))
vi.mock('~/composables/apis/useBillingApi', () => ({ useBillingApi: () => ({}) }))
vi.mock('~/composables/apis/useAuthApi', () => ({ useAuthApi: () => ({}) }))
vi.mock('qrcode', () => ({ default: { toDataURL: async (): Promise<string> => '' } }))

describe('EmailChangeDialog', () => {
  it('asks for the new address and the current password', () => {
    const wrapper = mountAccount(EmailChangeDialog, { props: { modelValue: true } })

    expect(wrapper.find('[data-testid="account-email-change-new"]').attributes('type')).toBe('email')
    expect(wrapper.find('[data-testid="account-email-change-password"]').attributes('type')).toBe('password')
  })

  it('sends the change and closes on success', async () => {
    const wrapper = mountAccount(EmailChangeDialog, { props: { modelValue: true } })
    const start = vi.spyOn(useAccountStore(), 'startEmailChange').mockResolvedValue(ActionOutcome.Succeeded)

    await wrapper.find('[data-testid="account-email-change-new"]').setValue(' new@example.com ')
    await wrapper.find('[data-testid="account-email-change-password"]').setValue('secret-pw')
    await wrapper.find('form').trigger('submit')
    await Promise.resolve()

    expect(start).toHaveBeenCalledWith({ newEmail: 'new@example.com', currentPassword: 'secret-pw' })
    expect(wrapper.emitted('update:modelValue')?.at(-1)).toEqual([false])
  })

  it('refuses a malformed address and a missing password without a request', async () => {
    const wrapper = mountAccount(EmailChangeDialog, { props: { modelValue: true } })
    const start = vi.spyOn(useAccountStore(), 'startEmailChange').mockResolvedValue(ActionOutcome.Succeeded)

    await wrapper.find('[data-testid="account-email-change-new"]').setValue('new.example.com')
    await wrapper.find('form').trigger('submit')
    await wrapper.vm.$nextTick()

    expect(start).not.toHaveBeenCalled()
    expect(wrapper.text()).toContain('client.profile.validation.emailInvalid')
    expect(wrapper.text()).toContain('client.emailChange.passwordRequired')
  })

  it('stays open and shows the API sentence when the change is refused', async () => {
    const wrapper = mountAccount(EmailChangeDialog, { props: { modelValue: true } })
    vi.spyOn(useAccountStore(), 'startEmailChange').mockResolvedValue(ActionOutcome.Failed)
    useAccountStore().emailChangeError = 'The current password is not correct.'

    await wrapper.find('[data-testid="account-email-change-new"]').setValue('new@example.com')
    await wrapper.find('[data-testid="account-email-change-password"]').setValue('wrong')
    await wrapper.find('form').trigger('submit')
    await Promise.resolve()
    await wrapper.vm.$nextTick()

    expect(wrapper.emitted('update:modelValue')).toBeUndefined()
    expect(wrapper.text()).toContain('The current password is not correct.')
  })
})

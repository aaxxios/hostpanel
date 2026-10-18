/**
 * Unit tests for TwoFactorDialog: enable vs disable content and the code submission.
 */
import { describe, expect, it, vi } from 'vitest'
import { mountAccount } from '~/tests/helpers/mountAccount'
import TwoFactorDialog from '~/components/account/TwoFactorDialog.vue'
import OtpInput from '~/components/ui/UiOtpInput.vue'
import { useAccountStore } from '~/stores/account'
import { ActionOutcome } from '~/types/actionoutcome'
import { TwoFactorMode } from '~/types/twofactormode'

vi.mock('~/composables/apis/useClientApi', () => ({ useClientApi: () => ({}) }))
vi.mock('~/composables/apis/useBillingApi', () => ({ useBillingApi: () => ({}) }))
vi.mock('~/composables/apis/useAuthApi', () => ({ useAuthApi: () => ({}) }))
vi.mock('qrcode', () => ({ default: { toDataURL: async (): Promise<string> => '' } }))

describe('TwoFactorDialog', () => {
  it('renders nothing while closed', () => {
    const wrapper = mountAccount(TwoFactorDialog, { props: { modelValue: false, mode: TwoFactorMode.Enable } })

    expect(wrapper.find('[data-testid="account-2fa-dialog"]').exists()).toBe(false)
  })

  it('shows the QR and secret when enabling', () => {
    const wrapper = mountAccount(TwoFactorDialog, {
      props: { modelValue: true, mode: TwoFactorMode.Enable, secret: 'ABCD', qrDataUrl: 'data:image/png;base64,QR' }
    })

    expect(wrapper.find('[data-testid="account-2fa-qr"]').attributes('src')).toBe('data:image/png;base64,QR')
    expect(wrapper.find('[data-testid="account-2fa-secret"]').text()).toContain('ABCD')
    expect(wrapper.text()).toContain('client.twoFactor.setupTitle')
  })

  it('shows neither QR nor secret when disabling', () => {
    const wrapper = mountAccount(TwoFactorDialog, {
      props: { modelValue: true, mode: TwoFactorMode.Disable, secret: 'ABCD', qrDataUrl: 'data:image/png;base64,QR' }
    })

    expect(wrapper.find('[data-testid="account-2fa-qr"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="account-2fa-secret"]').exists()).toBe(false)
    expect(wrapper.text()).toContain('client.twoFactor.disableTitle')
  })

  it('puts the submit button in the pinned footer, tied to the form by id', () => {
    const wrapper = mountAccount(TwoFactorDialog, { props: { modelValue: true, mode: TwoFactorMode.Enable } })
    const button = wrapper.find('[data-testid="account-2fa-submit"]')

    expect(wrapper.find('[data-testid="ui-dialog-body"]').find('[data-testid="account-2fa-submit"]').exists()).toBe(false)
    expect(button.attributes('type')).toBe('submit')
    expect(button.attributes('form')).toBe(wrapper.find('[data-testid="two-factor-form"]').attributes('id'))
  })

  it('submits the typed code for the mode and closes on success', async () => {
    const wrapper = mountAccount(TwoFactorDialog, { props: { modelValue: true, mode: TwoFactorMode.Disable } })
    const submit = vi.spyOn(useAccountStore(), 'submitTwoFactor').mockResolvedValue(ActionOutcome.Succeeded)

    wrapper.findComponent(OtpInput).vm.$emit('update:modelValue', '123456')
    await wrapper.vm.$nextTick()
    await wrapper.find('form').trigger('submit')
    await Promise.resolve()

    expect(submit).toHaveBeenCalledWith(TwoFactorMode.Disable, '123456')
    expect(wrapper.emitted('update:modelValue')?.at(-1)).toEqual([false])
  })

  it('stays open when the code is refused', async () => {
    const wrapper = mountAccount(TwoFactorDialog, { props: { modelValue: true, mode: TwoFactorMode.Enable } })
    vi.spyOn(useAccountStore(), 'submitTwoFactor').mockResolvedValue(ActionOutcome.Failed)

    await wrapper.find('form').trigger('submit')
    await Promise.resolve()

    expect(wrapper.emitted('update:modelValue')).toBeUndefined()
  })

  it('shows the refusal sentence', async () => {
    const wrapper = mountAccount(TwoFactorDialog, { props: { modelValue: true, mode: TwoFactorMode.Enable } })
    useAccountStore().tfaModalError = 'Wrong code'
    await wrapper.vm.$nextTick()

    expect(wrapper.find('[data-testid="ui-form-error"]').text()).toBe('Wrong code')
  })
})

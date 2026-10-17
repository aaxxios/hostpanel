/**
 * Unit tests for TwoFactorCard: state wording, enrolment start and the disable flow.
 */
import { describe, expect, it, vi } from 'vitest'
import { mountAccount } from '~/tests/helpers/mountAccount'
import TwoFactorCard from '~/components/account/TwoFactorCard.vue'
import { useAccountStore } from '~/stores/account'

vi.mock('~/composables/apis/useClientApi', () => ({ useClientApi: () => ({}) }))
vi.mock('~/composables/apis/useBillingApi', () => ({ useBillingApi: () => ({}) }))
vi.mock('~/composables/apis/useAuthApi', () => ({ useAuthApi: () => ({}) }))
vi.mock('qrcode', () => ({ default: { toDataURL: async (): Promise<string> => '' } }))

describe('TwoFactorCard', () => {
  it('offers to enable when two-factor is off', () => {
    const wrapper = mountAccount(TwoFactorCard, { props: { enabled: false } })

    expect(wrapper.find('[data-testid="account-2fa-enable"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="account-2fa-disable"]').exists()).toBe(false)
  })

  it('offers to disable when two-factor is on', () => {
    const wrapper = mountAccount(TwoFactorCard, { props: { enabled: true } })

    expect(wrapper.find('[data-testid="account-2fa-disable"]').exists()).toBe(true)
    expect(wrapper.text()).toContain('client.twoFactor.descEnabled')
  })

  it('opens the dialog with the secret once enrolment has started', async () => {
    const wrapper = mountAccount(TwoFactorCard, { props: { enabled: false } })
    const start = vi.spyOn(useAccountStore(), 'startTwoFactorSetup')
      .mockResolvedValue({ secret: 'ABCD', qrCodeUri: 'otpauth://x', qrDataUrl: 'data:image/png;base64,QR' })

    await wrapper.find('[data-testid="account-2fa-enable"]').trigger('click')
    await wrapper.vm.$nextTick()

    expect(start).toHaveBeenCalled()
    expect(wrapper.find('[data-testid="account-2fa-secret"]').text()).toContain('ABCD')
  })

  it('keeps the dialog closed when enrolment could not start', async () => {
    const wrapper = mountAccount(TwoFactorCard, { props: { enabled: false } })
    vi.spyOn(useAccountStore(), 'startTwoFactorSetup').mockResolvedValue(null)

    await wrapper.find('[data-testid="account-2fa-enable"]').trigger('click')
    await wrapper.vm.$nextTick()

    expect(wrapper.find('[data-testid="account-2fa-dialog"]').exists()).toBe(false)
  })

  it('opens the dialog without a QR when disabling', async () => {
    const wrapper = mountAccount(TwoFactorCard, { props: { enabled: true } })

    await wrapper.find('[data-testid="account-2fa-disable"]').trigger('click')

    expect(wrapper.find('[data-testid="account-2fa-dialog"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="account-2fa-qr"]').exists()).toBe(false)
  })
})

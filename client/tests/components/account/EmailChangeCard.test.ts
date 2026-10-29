/**
 * Unit tests for EmailChangeCard: the current address, the dialog it opens and the "check your
 * new inbox" state once a link has been sent.
 */
import { describe, expect, it, vi } from 'vitest'
import { mountAccount } from '~/tests/helpers/mountAccount'
import EmailChangeCard from '~/components/account/EmailChangeCard.vue'
import { useAccountStore } from '~/stores/account'

vi.mock('~/composables/apis/useClientApi', () => ({ useClientApi: () => ({}) }))
vi.mock('~/composables/apis/useBillingApi', () => ({ useBillingApi: () => ({}) }))
vi.mock('~/composables/apis/useAuthApi', () => ({ useAuthApi: () => ({}) }))
vi.mock('qrcode', () => ({ default: { toDataURL: async (): Promise<string> => '' } }))

describe('EmailChangeCard', () => {
  it('shows the address the account signs in with', () => {
    const wrapper = mountAccount(EmailChangeCard, { props: { email: 'ada@example.com' } })

    expect(wrapper.find('[data-testid="account-email-change-current"]').text()).toBe('ada@example.com')
    expect(wrapper.find('[data-testid="account-email-change-sent"]').exists()).toBe(false)
  })

  it('opens the change dialog', async () => {
    const wrapper = mountAccount(EmailChangeCard, { props: { email: 'ada@example.com' } })

    await wrapper.find('[data-testid="account-email-change-open"]').trigger('click')

    expect(wrapper.find('[data-testid="account-email-change-dialog"]').exists()).toBe(true)
  })

  it('tells the person to check the new inbox once a link went out', async () => {
    const wrapper = mountAccount(EmailChangeCard, { props: { email: 'ada@example.com' } })
    useAccountStore().emailChangeSentTo = 'new@example.com'
    await wrapper.vm.$nextTick()

    expect(wrapper.find('[data-testid="account-email-change-sent"]').text()).toContain('client.emailChange.sentTitle')
  })
})

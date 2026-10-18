/**
 * Unit tests for TeamTab: it shows the stored team and re-reads it after a change.
 */
import { describe, expect, it, vi } from 'vitest'
import { mountAccount } from '~/tests/helpers/mountAccount'
import TeamTab from '~/components/account/TeamTab.vue'
import { useAccountStore } from '~/stores/account'
import { ActionOutcome } from '~/types/actionoutcome'

vi.mock('~/composables/apis/useClientApi', () => ({ useClientApi: () => ({}) }))
vi.mock('~/composables/apis/useBillingApi', () => ({ useBillingApi: () => ({}) }))
vi.mock('~/composables/apis/useAuthApi', () => ({ useAuthApi: () => ({}) }))
vi.mock('qrcode', () => ({ default: { toDataURL: async (): Promise<string> => '' } }))

describe('TeamTab', () => {
  it('renders the list and the invitation form', () => {
    const wrapper = mountAccount(TeamTab)

    expect(wrapper.find('[data-testid="account-team-list"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="account-invite-card"]').exists()).toBe(true)
  })

  it('lists the members held by the store', async () => {
    const wrapper = mountAccount(TeamTab)
    useAccountStore().users = [{ userId: '2', firstName: 'Helper', lastName: 'Member', email: 'helper@example.com', isOwner: false, permissions: 8191, lastLoginAt: null, createdAt: '2029-03-01T00:00:00Z' }]
    await wrapper.vm.$nextTick()

    expect(wrapper.find('[data-testid="account-team-member-2"]').exists()).toBe(true)
  })

  it('reads the team again after an invitation went out', async () => {
    const wrapper = mountAccount(TeamTab)
    const store = useAccountStore()
    const reload = vi.spyOn(store, 'loadUsers').mockResolvedValue()
    vi.spyOn(store, 'inviteUser').mockResolvedValue(ActionOutcome.Succeeded)

    await wrapper.find('form').trigger('submit')
    await Promise.resolve()

    expect(reload).toHaveBeenCalled()
  })
})

/**
 * Unit tests for TeamMembersList: states, the owner row and the confirmed removal.
 */
import { describe, expect, it, vi } from 'vitest'
import { mountAccount } from '~/tests/helpers/mountAccount'
import TeamMembersList from '~/components/account/TeamMembersList.vue'
import { useAccountStore } from '~/stores/account'
import { ActionOutcome } from '~/types/actionoutcome'
import type { TeamMember } from '~/types/teammember'

vi.mock('~/composables/apis/useClientApi', () => ({ useClientApi: () => ({}) }))
vi.mock('~/composables/apis/useBillingApi', () => ({ useBillingApi: () => ({}) }))
vi.mock('~/composables/apis/useAuthApi', () => ({ useAuthApi: () => ({}) }))
vi.mock('qrcode', () => ({ default: { toDataURL: async (): Promise<string> => '' } }))

/** An owner and one removable member. */
const users: TeamMember[] = [
  {
    userId: '1', firstName: 'Olive', lastName: 'Owner', email: 'owner@example.com', isOwner: true,
    permissions: 8191, lastLoginAt: '2026-01-01T00:00:00Z', createdAt: '2025-01-01T00:00:00Z'
  },
  {
    userId: '2', firstName: 'Hal', lastName: 'Helper', email: 'helper@example.com', isOwner: false,
    permissions: 4, lastLoginAt: null, createdAt: '2025-06-01T00:00:00Z'
  }
]

describe('TeamMembersList', () => {
  it('shows a skeleton while loading', () => {
    const wrapper = mountAccount(TeamMembersList, { props: { users: [], loading: true } })

    expect(wrapper.find('[data-testid="account-team-loading"]').exists()).toBe(true)
  })

  it('shows the empty state without members', () => {
    const wrapper = mountAccount(TeamMembersList, { props: { users: [], loading: false } })

    expect(wrapper.find('[data-testid="account-team-empty"]').exists()).toBe(true)
  })

  it('lists members, the owner without a remove button', () => {
    const wrapper = mountAccount(TeamMembersList, { props: { users, loading: false } })

    expect(wrapper.find('[data-testid="account-team-member-1"]').text()).toContain('client.users.owner')
    expect(wrapper.find('[data-testid="account-team-remove-1"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="account-team-remove-2"]').exists()).toBe(true)
  })

  it('removes a member only after confirmation, then asks for a fresh list', async () => {
    const wrapper = mountAccount(TeamMembersList, { props: { users, loading: false } })
    const remove = vi.spyOn(useAccountStore(), 'removeUser').mockResolvedValue(ActionOutcome.Succeeded)

    await wrapper.find('[data-testid="account-team-remove-2"]').trigger('click')
    expect(remove).not.toHaveBeenCalled()

    await wrapper.find('[data-testid="ui-confirm-confirm"]').trigger('click')
    await Promise.resolve()

    expect(remove).toHaveBeenCalledWith('2')
    expect(wrapper.emitted('changed')).toHaveLength(1)
  })

  it('does not ask for a fresh list when the removal is refused', async () => {
    const wrapper = mountAccount(TeamMembersList, { props: { users, loading: false } })
    vi.spyOn(useAccountStore(), 'removeUser').mockResolvedValue(ActionOutcome.Failed)

    await wrapper.find('[data-testid="account-team-remove-2"]').trigger('click')
    await wrapper.find('[data-testid="ui-confirm-confirm"]').trigger('click')
    await Promise.resolve()

    expect(wrapper.emitted('changed')).toBeUndefined()
  })
})

/**
 * Unit tests for InviteUserForm: the permission modes, the payload and the reset after success.
 */
import { describe, expect, it, vi } from 'vitest'
import { mountAccount } from '~/tests/helpers/mountAccount'
import InviteUserForm from '~/components/account/InviteUserForm.vue'
import { useAccountStore } from '~/stores/account'
import { ActionOutcome } from '~/types/actionoutcome'

vi.mock('~/composables/apis/useClientApi', () => ({ useClientApi: () => ({}) }))
vi.mock('~/composables/apis/useBillingApi', () => ({ useBillingApi: () => ({}) }))
vi.mock('~/composables/apis/useAuthApi', () => ({ useAuthApi: () => ({}) }))
vi.mock('qrcode', () => ({ default: { toDataURL: async (): Promise<string> => '' } }))

/**
 * Fills the three identity fields.
 *
 * @param wrapper - The mounted form.
 */
const fill = async (wrapper: ReturnType<typeof mountAccount>): Promise<void> => {
  await wrapper.find('[data-testid="account-invite-firstname"]').setValue('Jane')
  await wrapper.find('[data-testid="account-invite-lastname"]').setValue('Doe')
  await wrapper.find('[data-testid="account-invite-email"]').setValue('jane@example.com')
}

describe('InviteUserForm', () => {
  it('gives every field a placeholder', () => {
    const wrapper = mountAccount(InviteUserForm)

    for (const input of wrapper.findAll('input[type="text"], input[type="email"]')) {
      expect(input.attributes('placeholder')).toBeTruthy()
    }
  })

  it('hides the permission list until "choose" is selected', async () => {
    const wrapper = mountAccount(InviteUserForm)
    expect(wrapper.find('[data-testid="account-invite-perms"]').exists()).toBe(false)

    await wrapper.find('[data-testid="account-invite-perm-choose"]').trigger('click')

    expect(wrapper.find('[data-testid="account-invite-perms"]').exists()).toBe(true)
    expect(wrapper.findAll('[data-testid="account-invite-perms"] [role="checkbox"]')).toHaveLength(13)
  })

  it('sends "all" permissions by default', async () => {
    const wrapper = mountAccount(InviteUserForm)
    const invite = vi.spyOn(useAccountStore(), 'inviteUser').mockResolvedValue(ActionOutcome.Succeeded)

    await fill(wrapper)
    await wrapper.find('form').trigger('submit')
    await Promise.resolve()

    expect(invite).toHaveBeenCalledWith({
      email: 'jane@example.com', firstName: 'Jane', lastName: 'Doe', permissions: 'all'
    })
  })

  it('sends only the ticked permissions when choosing', async () => {
    const wrapper = mountAccount(InviteUserForm)
    const invite = vi.spyOn(useAccountStore(), 'inviteUser').mockResolvedValue(ActionOutcome.Succeeded)

    await fill(wrapper)
    await wrapper.find('[data-testid="account-invite-perm-choose"]').trigger('click')
    await wrapper.find('[data-testid="account-invite-perm-tickets"]').trigger('click')
    await wrapper.find('[data-testid="account-invite-perm-invoices"]').trigger('click')
    await wrapper.find('form').trigger('submit')
    await Promise.resolve()

    expect(invite).toHaveBeenCalledWith(expect.objectContaining({ permissions: 'invoices,tickets' }))
  })

  it('empties the form, reports success and asks for a fresh list', async () => {
    const wrapper = mountAccount(InviteUserForm)
    vi.spyOn(useAccountStore(), 'inviteUser').mockResolvedValue(ActionOutcome.Succeeded)

    await fill(wrapper)
    await wrapper.find('form').trigger('submit')
    await Promise.resolve()
    await wrapper.vm.$nextTick()

    expect((wrapper.find('[data-testid="account-invite-email"]').element as HTMLInputElement).value).toBe('')
    expect(wrapper.find('[data-testid="ui-form-success"]').text()).toBe('client.users.inviteSent')
    expect(wrapper.emitted('invited')).toHaveLength(1)
  })

  it('keeps the form and emits nothing when the invitation is refused', async () => {
    const wrapper = mountAccount(InviteUserForm)
    vi.spyOn(useAccountStore(), 'inviteUser').mockResolvedValue(ActionOutcome.Failed)

    await fill(wrapper)
    await wrapper.find('form').trigger('submit')
    await Promise.resolve()

    expect((wrapper.find('[data-testid="account-invite-email"]').element as HTMLInputElement).value).toBe('jane@example.com')
    expect(wrapper.emitted('invited')).toBeUndefined()
  })
})

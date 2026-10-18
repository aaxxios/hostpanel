/**
 * Unit tests for ContactsTab: states, opening the dialog and the confirmed deletion.
 */
import { describe, expect, it, vi } from 'vitest'
import { mountAccount } from '~/tests/helpers/mountAccount'
import ContactsTab from '~/components/account/ContactsTab.vue'
import { useAccountStore } from '~/stores/account'
import { ActionOutcome } from '~/types/actionoutcome'
import type { AccountContact } from '~/types/accountcontact'
import { makeContact } from '~/tests/helpers/accountContact'

vi.mock('~/composables/apis/useClientApi', () => ({ useClientApi: () => ({}) }))
vi.mock('~/composables/apis/useBillingApi', () => ({ useBillingApi: () => ({}) }))
vi.mock('~/composables/apis/useAuthApi', () => ({ useAuthApi: () => ({}) }))
vi.mock('qrcode', () => ({ default: { toDataURL: async (): Promise<string> => '' } }))

/** One saved contact. */
const contact: AccountContact = makeContact({ id: 7, companyName: 'Acme' })

describe('ContactsTab', () => {
  it('shows a skeleton while loading', async () => {
    const wrapper = mountAccount(ContactsTab)
    useAccountStore().contactsLoading = true
    await wrapper.vm.$nextTick()

    expect(wrapper.find('[data-testid="account-contacts-loading"]').exists()).toBe(true)
  })

  it('shows the empty state without contacts', () => {
    const wrapper = mountAccount(ContactsTab)

    expect(wrapper.find('[data-testid="ui-empty-state"]').text()).toContain('client.contacts.empty')
  })

  it('lists the contacts of the store', async () => {
    const wrapper = mountAccount(ContactsTab)
    useAccountStore().contacts = [contact]
    await wrapper.vm.$nextTick()

    expect(wrapper.find('[data-testid="account-contact-7"]').text()).toContain('Ann Lee')
    expect(wrapper.find('[data-testid="account-contact-7"]').text()).toContain('Acme')
  })

  it('opens the dialog empty from "add"', async () => {
    const wrapper = mountAccount(ContactsTab)

    await wrapper.find('[data-testid="account-contact-add"]').trigger('click')

    expect(wrapper.text()).toContain('client.contacts.addTitle')
  })

  it('opens the dialog filled from "edit"', async () => {
    const wrapper = mountAccount(ContactsTab)
    useAccountStore().contacts = [contact]
    await wrapper.vm.$nextTick()

    await wrapper.find('[data-testid="account-contact-edit-7"]').trigger('click')

    expect(wrapper.text()).toContain('client.contacts.editTitle')
    expect((wrapper.find('[data-testid="account-contact-firstname"]').element as HTMLInputElement).value).toBe('Ann')
  })

  it('deletes only after confirmation and then reads the list again', async () => {
    const wrapper = mountAccount(ContactsTab)
    const store = useAccountStore()
    store.contacts = [contact]
    const remove = vi.spyOn(store, 'removeContact').mockResolvedValue(ActionOutcome.Succeeded)
    const reload = vi.spyOn(store, 'loadContacts').mockResolvedValue()
    await wrapper.vm.$nextTick()

    await wrapper.find('[data-testid="account-contact-delete-7"]').trigger('click')
    expect(remove).not.toHaveBeenCalled()

    await wrapper.find('[data-testid="ui-confirm-confirm"]').trigger('click')
    await Promise.resolve()

    expect(remove).toHaveBeenCalledWith(7)
    expect(reload).toHaveBeenCalled()
  })

  it('keeps the list when the deletion fails', async () => {
    const wrapper = mountAccount(ContactsTab)
    const store = useAccountStore()
    store.contacts = [contact]
    vi.spyOn(store, 'removeContact').mockResolvedValue(ActionOutcome.Failed)
    const reload = vi.spyOn(store, 'loadContacts').mockResolvedValue()
    await wrapper.vm.$nextTick()

    await wrapper.find('[data-testid="account-contact-delete-7"]').trigger('click')
    await wrapper.find('[data-testid="ui-confirm-confirm"]').trigger('click')
    await Promise.resolve()

    expect(reload).not.toHaveBeenCalled()
  })
})

/**
 * Unit tests for ContactDialog: add vs edit seeding, validation, saving and closing.
 */
import { describe, expect, it, vi } from 'vitest'
import { mountAccount } from '~/tests/helpers/mountAccount'
import ContactDialog from '~/components/account/ContactDialog.vue'
import { useAccountStore } from '~/stores/account'
import { ContactSaveOutcome } from '~/types/contactsaveoutcome'
import type { AccountContact } from '~/types/accountcontact'
import { ContactType } from '~/types/contacttype'
import { makeContact } from '~/tests/helpers/accountContact'

vi.mock('~/composables/apis/useClientApi', () => ({ useClientApi: () => ({}) }))
vi.mock('~/composables/apis/useBillingApi', () => ({ useBillingApi: () => ({}) }))
vi.mock('~/composables/apis/useAuthApi', () => ({ useAuthApi: () => ({}) }))
vi.mock('qrcode', () => ({ default: { toDataURL: async (): Promise<string> => '' } }))

/** A contact with the support mail switched off. */
const contact: AccountContact = makeContact({
  id: 7, phone: '555', type: ContactType.Billing,
  address: { street: '1 Pier', address2: null, city: 'Arlington', state: null, postCode: null, country: null },
  notifications: { general: true, invoice: false, support: false, product: true, domain: true, affiliate: false }
})

describe('ContactDialog', () => {
  it('titles the dialog "add" without a contact and starts empty', () => {
    const wrapper = mountAccount(ContactDialog, { props: { modelValue: true, contact: null } })

    expect(wrapper.text()).toContain('client.contacts.addTitle')
    expect((wrapper.find('[data-testid="account-contact-firstname"]').element as HTMLInputElement).value).toBe('')
  })

  it('titles the dialog "edit" and seeds the draft from the contact', () => {
    const wrapper = mountAccount(ContactDialog, { props: { modelValue: true, contact } })

    expect(wrapper.text()).toContain('client.contacts.editTitle')
    expect((wrapper.find('[data-testid="account-contact-email"]').element as HTMLInputElement).value).toBe('ann@example.com')
    expect(wrapper.find('[data-testid="account-contact-invoice"]').attributes('aria-checked')).toBe('false')
  })

  it('gives every text field a placeholder', () => {
    const wrapper = mountAccount(ContactDialog, { props: { modelValue: true, contact: null } })

    for (const input of wrapper.findAll('input[type="text"], input[type="email"], input[type="tel"]')) {
      expect(input.attributes('placeholder')).toBeTruthy()
    }
  })

  it('creates when there is no contact, then closes and announces the save', async () => {
    const wrapper = mountAccount(ContactDialog, { props: { modelValue: true, contact: null } })
    const save = vi.spyOn(useAccountStore(), 'saveContact').mockResolvedValue(ContactSaveOutcome.Saved)

    await wrapper.find('[data-testid="account-contact-firstname"]').setValue('Ann')
    await wrapper.find('form').trigger('submit')
    await Promise.resolve()

    expect(save).toHaveBeenCalledWith(null, expect.objectContaining({ firstName: 'Ann', phone: null, address: expect.objectContaining({ street: null }) }))
    expect(wrapper.emitted('saved')).toHaveLength(1)
    expect(wrapper.emitted('update:modelValue')?.at(-1)).toEqual([false])
  })

  it('updates by id when editing', async () => {
    const wrapper = mountAccount(ContactDialog, { props: { modelValue: true, contact } })
    const save = vi.spyOn(useAccountStore(), 'saveContact').mockResolvedValue(ContactSaveOutcome.Saved)

    await wrapper.find('form').trigger('submit')
    await Promise.resolve()

    expect(save).toHaveBeenCalledWith(7, expect.objectContaining({
      lastName: 'Lee', type: ContactType.Billing, companyName: null,
      notifications: expect.objectContaining({ support: false }),
      address: expect.objectContaining({ street: '1 Pier', city: 'Arlington' })
    }))
  })

  it('shows the local sentence when the name is missing and stays open', async () => {
    const wrapper = mountAccount(ContactDialog, { props: { modelValue: true, contact: null } })
    vi.spyOn(useAccountStore(), 'saveContact').mockResolvedValue(ContactSaveOutcome.Invalid)

    await wrapper.find('form').trigger('submit')
    await Promise.resolve()
    await wrapper.vm.$nextTick()

    expect(wrapper.find('[data-testid="ui-form-error"]').text()).toBe('client.contacts.saveError')
    expect(wrapper.emitted('saved')).toBeUndefined()
  })

  it('closes from the cancel button without saving', async () => {
    const wrapper = mountAccount(ContactDialog, { props: { modelValue: true, contact: null } })

    await wrapper.find('[data-testid="account-contact-cancel"]').trigger('click')

    expect(wrapper.emitted('update:modelValue')?.at(-1)).toEqual([false])
    expect(wrapper.emitted('saved')).toBeUndefined()
  })
})

/**
 * Mount tests for ContactFormModal: create mode, edit mode, validation and the delete prompt.
 */
import { describe, expect, it } from 'vitest'
import { flushPromises, mount } from '@vue/test-utils'
import ContactFormModal from '@/components/clients/ContactFormModal.vue'
import type { Contact } from '@/types/contact'
import { createTestI18n } from '../../helpers/i18n'

/**
 * Builds a contact fixture.
 *
 * @returns A contact with every notification on.
 */
const contact = (): Contact =>
  ({
    id: 1,
    firstName: 'Ada',
    lastName: 'Lovelace',
    companyName: null,
    email: 'ada@example.com',
    phone: null,
    type: 'Billing',
    street: null,
    address2: null,
    city: null,
    state: null,
    postCode: null,
    country: null,
    notifyGeneral: true,
    notifyInvoice: false,
    notifySupport: true,
    notifyProduct: true,
    notifyDomain: true,
    notifyAffiliate: true,
  }) as unknown as Contact

/**
 * Mounts the modal with the dialog teleport inlined.
 *
 * @param value - Contact to edit, or null to create.
 * @returns The wrapper.
 */
const mountModal = (value: Contact | null) =>
  mount(ContactFormModal, {
    props: { contact: value, saving: false },
    global: { plugins: [createTestI18n()], stubs: { teleport: true } },
  })

describe('ContactFormModal', () => {
  it('refuses to save an empty form', async () => {
    const wrapper = mountModal(null)

    await wrapper.get('form').trigger('submit')

    expect(wrapper.emitted('save')).toBeUndefined()
    expect(wrapper.findAll('[role="alert"]')).toHaveLength(3)
  })

  it('emits a payload with the typed names', async () => {
    const wrapper = mountModal(null)

    await wrapper.get('[data-testid="clients-contact-first-name"]').setValue('Grace')
    await wrapper.get('[data-testid="clients-contact-last-name"]').setValue('Hopper')
    await wrapper.get('[data-testid="clients-contact-email"]').setValue('grace@example.com')
    await wrapper.get('form').trigger('submit')

    const payload = wrapper.emitted('save')?.[0]?.[0] as Record<string, unknown>
    expect(payload.firstName).toBe('Grace')
    expect(payload.phone).toBeNull()
    expect(payload.notifyGeneral).toBe(true)
  })

  it('fills the form from the contact and offers delete in edit mode', async () => {
    const wrapper = mountModal(contact())
    await flushPromises()

    expect((wrapper.get('[data-testid="clients-contact-first-name"]').element as HTMLInputElement).value).toBe('Ada')
    expect(wrapper.find('[data-testid="clients-contact-delete"]').exists()).toBe(true)
  })

  it('asks before deleting and emits delete on confirm', async () => {
    const wrapper = mountModal(contact())

    await wrapper.get('[data-testid="clients-contact-delete"]').trigger('click')
    await wrapper.get('[data-testid="dialog-confirm"]').trigger('click')

    expect(wrapper.emitted('delete')).toHaveLength(1)
  })

  it('has no delete button in create mode', () => {
    expect(mountModal(null).find('[data-testid="clients-contact-delete"]').exists()).toBe(false)
  })
})

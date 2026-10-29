/**
 * Unit tests for DomainContactPanel: the registry record, pre-filling the registrant from the
 * profile or a contact, editing, cancelling and saving.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { mountClient } from '~/tests/helpers/mountClient'
import DomainContactPanel from '~/components/client/domains/DomainContactPanel.vue'
import { ContactType } from '~/types/contacttype'
import { makeClientUser } from '~/tests/helpers/clientUser'

/** The mocked endpoints the store reaches. */
const domainsApi = vi.hoisted(() => ({ fetchWhois: vi.fn(), fetchContact: vi.fn(), setWhois: vi.fn() }))

/** The mocked client endpoints the store reaches. */
const clientApi = vi.hoisted(() => ({ fetchContacts: vi.fn(), fetchMeOnce: vi.fn() }))

vi.mock('~/composables/apis/useDomainsApi', () => ({ useDomainsApi: () => domainsApi }))
vi.mock('~/composables/apis/useClientApi', () => ({ useClientApi: () => clientApi }))
vi.mock('~/composables/apis/useBillingApi', () => ({ useBillingApi: () => ({}) }))
vi.mock('~/composables/apis/useCatalogApi', () => ({ useCatalogApi: () => ({}) }))
vi.mock('~/composables/apis/useCurrencyApi', () => ({ useCurrencyApi: () => ({}) }))

/** The registry record the API returns. */
const whois = { registrar: 'Reg Ltd', registrant: 'Ada Lovelace', createdAt: '2026-01-01T00:00:00Z', expiresAt: '2027-01-01T00:00:00Z' }

/** The registrant contact the registrar holds, as the backend answers it. */
const registrant = {
  firstName: 'Margaret',
  lastName: 'Hamilton',
  organization: null,
  email: 'margaret@example.com',
  phone: '777',
  address1: '2 Apollo Way',
  address2: null,
  city: 'Boston',
  state: 'MA',
  postalCode: '02101',
  country: 'US'
}

/** A contact as the backend lists it. */
const contact = {
  id: 5,
  firstName: 'Grace',
  lastName: 'Hopper',
  companyName: 'Navy',
  email: 'grace@example.com',
  phone: '555',
  type: ContactType.General,
  address: { street: '1 Pier', address2: null, city: 'Arlington', state: null, postCode: '22201', country: 'US' },
  notifications: { general: true, invoice: true, support: true, product: true, domain: false, affiliate: false }
}

/**
 * Reads the value of a contact field.
 *
 * @param wrapper - The mounted panel.
 * @param key - The field key.
 * @returns The field's current value.
 */
const field = (wrapper: ReturnType<typeof mountClient>, key: string): string =>
  (wrapper.find(`[data-testid="domain-contact-${key}"]`).element as HTMLInputElement).value

/**
 * Mounts the panel and waits for the profile to pre-fill the form.
 *
 * @returns The wrapper.
 */
const mountPanel = async () => {
  const wrapper = mountClient(DomainContactPanel, { props: { domainId: '7' } })
  await vi.waitFor(() => expect(field(wrapper, 'firstName')).toBe('Linus'))
  return wrapper
}

describe('DomainContactPanel', () => {
  beforeEach(() => {
    domainsApi.fetchWhois.mockReset()
    domainsApi.fetchContact.mockReset()
    domainsApi.setWhois.mockReset()
    clientApi.fetchContacts.mockReset()
    clientApi.fetchMeOnce.mockReset()
    domainsApi.fetchWhois.mockResolvedValue({ ...whois })
    // By default the registrar holds no contact, so the profile stands in.
    domainsApi.fetchContact.mockRejectedValue({ data: { detail: 'No contact.' } })
    clientApi.fetchContacts.mockResolvedValue([{ ...contact }])
    clientApi.fetchMeOnce.mockResolvedValue(makeClientUser({
      firstName: 'Linus',
      lastName: 'T',
      address: { street: null, address2: null, city: 'Helsinki', state: null, postCode: '00100', country: null }
    }))
  })

  it('shows the registry record and pre-fills the form from the primary profile', async () => {
    const wrapper = await mountPanel()

    expect(wrapper.find('[data-testid="domain-whois-record"]').text()).toContain('Reg Ltd')
    expect(wrapper.find('[data-testid="domain-whois-record"]').text()).toContain('Ada Lovelace')
    expect(field(wrapper, 'city')).toBe('Helsinki')
    expect(field(wrapper, 'postalCode')).toBe('00100')
    expect(wrapper.find('[data-testid="ui-tab-Tech"]').exists()).toBe(false)
  })

  it('pre-fills the form from the registrar contact instead of the profile when it is available', async () => {
    domainsApi.fetchContact.mockResolvedValue({ ...registrant })
    const wrapper = mountClient(DomainContactPanel, { props: { domainId: '7' } })

    await vi.waitFor(() => expect(field(wrapper, 'firstName')).toBe('Margaret'))
    expect(field(wrapper, 'city')).toBe('Boston')
    expect(field(wrapper, 'postalCode')).toBe('02101')
    expect(domainsApi.fetchContact).toHaveBeenCalledWith('7')
  })

  it('says the record is unavailable when the registry lookup fails, and still offers the form', async () => {
    domainsApi.fetchWhois.mockRejectedValue({ data: { detail: 'No record.' } })
    const wrapper = mountClient(DomainContactPanel, { props: { domainId: '7' } })

    await vi.waitFor(() => expect(wrapper.find('[data-testid="domain-whois-unavailable"]').exists()).toBe(true))
    await vi.waitFor(() => expect(field(wrapper, 'firstName')).toBe('Linus'))
  })

  it('locks the fields while an existing contact is selected and unlocks them for custom data', async () => {
    const wrapper = await mountPanel()

    expect(wrapper.find('[data-testid="domain-contact-firstName"]').attributes('disabled')).toBeDefined()

    await wrapper.find('[data-testid="domain-contact-custom"]').trigger('click')

    expect(wrapper.find('[data-testid="domain-contact-firstName"]').attributes('disabled')).toBeUndefined()
  })

  it('restores the loaded values when Cancel is pressed', async () => {
    const wrapper = await mountPanel()
    await wrapper.find('[data-testid="domain-contact-custom"]').trigger('click')
    await wrapper.find('[data-testid="domain-contact-firstName"]').setValue('Edited')

    await wrapper.find('[data-testid="domain-contact-cancel"]').trigger('click')

    expect(field(wrapper, 'firstName')).toBe('Linus')
  })

  it('saves the registrant contact and confirms', async () => {
    domainsApi.setWhois.mockResolvedValue({})
    const wrapper = await mountPanel()

    await wrapper.find('form').trigger('submit')

    await vi.waitFor(() => expect(domainsApi.setWhois).toHaveBeenCalledTimes(1))
    const [id, saved] = domainsApi.setWhois.mock.calls[0]!
    expect(id).toBe('7')
    expect(saved).toMatchObject({ firstName: 'Linus', lastName: 'T', city: 'Helsinki', postalCode: '00100' })
    await vi.waitFor(() => expect(wrapper.find('[data-testid="ui-form-success"]').exists()).toBe(true))
  })

  it('shows the refusal when the save fails', async () => {
    domainsApi.setWhois.mockRejectedValue({ data: { detail: 'Invalid phone.' } })
    const wrapper = await mountPanel()

    await wrapper.find('form').trigger('submit')

    await vi.waitFor(() => expect(wrapper.find('[data-testid="ui-form-error"]').text()).toContain('Invalid phone.'))
  })
})

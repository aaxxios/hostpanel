/**
 * Mount tests for AddTimeBillingModal: the row grid, validation and the save request.
 */
import { beforeEach, describe, expect, it } from 'vitest'
import { flushPromises, mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import AddTimeBillingModal from '@/components/clients/AddTimeBillingModal.vue'
import { createTestI18n } from '../../helpers/i18n'
import { json, problem, stubFetch } from '../../helpers/fetch'

/**
 * Mounts the modal for client 5.
 *
 * @returns The wrapper.
 */
const mountModal = () =>
  mount(AddTimeBillingModal, {
    props: { clientId: '5', saving: false },
    global: { plugins: [createTestI18n(), createPinia()], stubs: { teleport: true } },
  })

describe('AddTimeBillingModal', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  it('renders ten entry rows', async () => {
    stubFetch(json({ items: [] }))
    const wrapper = mountModal()
    await flushPromises()

    expect(wrapper.findAll('tbody tr')).toHaveLength(10)
  })

  it('refuses to save when no row is filled in', async () => {
    const fetchMock = stubFetch(json({ items: [] }))
    const wrapper = mountModal()
    await flushPromises()
    const calls = fetchMock.mock.calls.length

    await wrapper.get('[data-testid="clients-time-billing-save"]').trigger('click')

    expect(wrapper.get('[data-testid="clients-time-billing-error"]').text()).not.toBe('')
    expect(fetchMock.mock.calls.length).toBe(calls)
  })

  it('does not emit saved when the API rejects the entries', async () => {
    stubFetch(json({ items: [] }), problem('Refused by the server'))
    const wrapper = mountModal()
    await flushPromises()
    await wrapper.get('[data-testid="clients-time-billing-description-0"]').setValue('Consulting')
    const inputs = wrapper.findAll('tbody tr').at(0)?.findAll('input') ?? []
    await inputs.at(-2)?.setValue('2')

    await wrapper.get('[data-testid="clients-time-billing-save"]').trigger('click')
    await flushPromises()

    expect(wrapper.emitted('saved')).toBeUndefined()
  })

  it('emits close from cancel', async () => {
    stubFetch(json({ items: [] }))
    const wrapper = mountModal()

    await wrapper.get('[data-testid="clients-time-billing-cancel"]').trigger('click')

    expect(wrapper.emitted('close')).toHaveLength(1)
  })
})

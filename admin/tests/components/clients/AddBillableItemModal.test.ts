/**
 * Mount tests for AddBillableItemModal: save request and refusal.
 */
import { beforeEach, describe, expect, it } from 'vitest'
import { flushPromises, mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import AddBillableItemModal from '@/components/clients/AddBillableItemModal.vue'
import { createTestI18n } from '../../helpers/i18n'
import { json, problem, stubFetch, callOf } from '../../helpers/fetch'

/**
 * Mounts the modal for client 3.
 *
 * @returns The wrapper.
 */
const mountModal = () =>
  mount(AddBillableItemModal, {
    props: { clientId: '3', saving: false },
    global: { plugins: [createTestI18n(), createPinia()], stubs: { teleport: true } },
  })

describe('AddBillableItemModal', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  it('submits the description and emits saved', async () => {
    const fetchMock = stubFetch(json({ items: [] }), json({ id: 1 }))
    const wrapper = mountModal()
    await flushPromises()

    await wrapper.get('[data-testid="clients-add-billable-item-description"]').setValue('Setup fee')
    await wrapper.get('form').trigger('submit')
    await flushPromises()

    expect(callOf(fetchMock, 1).method).toBe('POST')
    expect((callOf(fetchMock, 1).body as { description: string }).description).toBe('Setup fee')
    expect(wrapper.emitted('saved')).toHaveLength(1)
  })

  it('shows the store refusal', async () => {
    stubFetch(json({ items: [] }), problem('Not allowed'))
    const wrapper = mountModal()
    await flushPromises()

    await wrapper.get('form').trigger('submit')
    await flushPromises()

    expect(wrapper.get('[data-testid="clients-add-billable-item-error"]').text()).toContain('Not allowed')
    expect(wrapper.emitted('saved')).toBeUndefined()
  })

  it('emits close from cancel', async () => {
    stubFetch(json({ items: [] }))
    const wrapper = mountModal()

    await wrapper.get('[data-testid="clients-add-billable-item-cancel"]').trigger('click')

    expect(wrapper.emitted('close')).toHaveLength(1)
  })
})

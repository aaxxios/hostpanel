/**
 * Mount tests for AddTransactionModal: form fields, save request and refusal.
 */
import { beforeEach, describe, expect, it } from 'vitest'
import { flushPromises, mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import AddTransactionModal from '@/components/clients/AddTransactionModal.vue'
import { createTestI18n } from '../../helpers/i18n'
import { json, problem, stubFetch, callOf } from '../../helpers/fetch'

/**
 * Mounts the modal for client 9.
 *
 * @returns The wrapper.
 */
const mountModal = () =>
  mount(AddTransactionModal, {
    props: { clientId: '9', saving: false },
    global: { plugins: [createTestI18n(), createPinia()], stubs: { teleport: true } },
  })

describe('AddTransactionModal', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  it('posts the typed description and emits saved', async () => {
    const fetchMock = stubFetch(json({ id: 1 }))
    const wrapper = mountModal()

    await wrapper.get('[data-testid="clients-add-transaction-description"]').setValue('Wire')
    await wrapper.get('form').trigger('submit')
    await flushPromises()

    const body = callOf(fetchMock).body as { description: string; clientId: number }
    expect(callOf(fetchMock).method).toBe('POST')
    expect(body.description).toBe('Wire')
    expect(body.clientId).toBe(9)
    expect(wrapper.emitted('saved')).toHaveLength(1)
  })

  it('shows the refusal sentence and stays open', async () => {
    stubFetch(problem('Amount is invalid'))
    const wrapper = mountModal()

    await wrapper.get('form').trigger('submit')
    await flushPromises()

    expect(wrapper.get('[data-testid="clients-add-transaction-error"]').text()).toContain('Amount is invalid')
    expect(wrapper.emitted('saved')).toBeUndefined()
  })

  it('emits close from cancel', async () => {
    const wrapper = mountModal()

    await wrapper.get('[data-testid="clients-add-transaction-cancel"]').trigger('click')

    expect(wrapper.emitted('close')).toHaveLength(1)
  })
})

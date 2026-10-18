/**
 * Tests for useReportClients: the picker list loads on mount and is empty when the call fails.
 */
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises, mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import { defineComponent, h } from 'vue'
import { useReportClients } from '@/composables/useReportClients'
import { json, problem } from '../helpers/fetch'
import { stubRouteTable } from '../helpers/routes'

/**
 * Mounts a host that prints the names of the clients it got.
 *
 * @returns The wrapper.
 */
const mountHost = () =>
  mount(
    defineComponent({
      setup() {
        const { clients } = useReportClients()
        return () => h('p', clients.value.map(client => client.name).join(','))
      },
    }),
    { global: { plugins: [createPinia()] } },
  )

describe('useReportClients', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('lists the clients the picker endpoint returns', async () => {
    stubRouteTable({ 'GET /reports/client-picker': () => json([{ id: 1, name: 'Acme' }, { id: 2, name: 'Beta' }]) })
    const wrapper = mountHost()
    await flushPromises()

    expect(wrapper.text()).toBe('Acme,Beta')
  })

  it('is empty when the call is refused', async () => {
    stubRouteTable({ 'GET /reports/client-picker': () => problem('No access.', 403) })
    const wrapper = mountHost()
    await flushPromises()

    expect(wrapper.text()).toBe('')
  })
})

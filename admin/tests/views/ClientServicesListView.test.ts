/**
 * Smoke mount test for ClientServicesListView: it renders against the real kit and settles on an API refusal.
 */
import { describe, expect, it, vi } from 'vitest'
import ClientServicesListView from '@/views/ClientServicesListView.vue'
import { mountClientView } from '../helpers/clientViews'
import { flushPromises } from '@vue/test-utils'

describe('ClientServicesListView', () => {
  it('mounts without throwing', async () => {
    const wrapper = await mountClientView(ClientServicesListView)

    expect(wrapper.exists()).toBe(true)
  })

  it('shows the refusal sentence from the API', async () => {
    const wrapper = await mountClientView(ClientServicesListView, 'Server said no')

    expect(wrapper.text()).toContain('Server said no')
  })

  it('loads again from the Retry of a refused load', async () => {
    const wrapper = await mountClientView(ClientServicesListView)
    await flushPromises()
    const fetchMock = vi.mocked(fetch)
    const before = fetchMock.mock.calls.length

    await wrapper.get('[data-testid="client-services-error-retry"]').trigger('click')
    await flushPromises()

    expect(fetchMock.mock.calls.length).toBeGreaterThan(before)
  })
})

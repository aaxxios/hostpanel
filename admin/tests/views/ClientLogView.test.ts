/**
 * Smoke mount test for ClientLogView: it renders against the real kit and settles on an API refusal.
 */
import { describe, expect, it, vi } from 'vitest'
import ClientLogView from '@/views/ClientLogView.vue'
import { mountClientView } from '../helpers/clientViews'
import { flushPromises } from '@vue/test-utils'

describe('ClientLogView', () => {
  it('mounts without throwing', async () => {
    const wrapper = await mountClientView(ClientLogView)

    expect(wrapper.exists()).toBe(true)
  })

  it('renders its main action', async () => {
    const wrapper = await mountClientView(ClientLogView)

    expect(wrapper.find('[data-testid="client-log-toggle-filters"]').exists()).toBe(true)
  })

  it('shows the refusal sentence from the API', async () => {
    const wrapper = await mountClientView(ClientLogView, 'Server said no')

    expect(wrapper.text()).toContain('Server said no')
  })

  it('loads again from the Retry of a refused load', async () => {
    const wrapper = await mountClientView(ClientLogView)
    await flushPromises()
    const fetchMock = vi.mocked(fetch)
    const before = fetchMock.mock.calls.length

    await wrapper.get('[data-testid="client-log-error-retry"]').trigger('click')
    await flushPromises()

    expect(fetchMock.mock.calls.length).toBeGreaterThan(before)
  })
})

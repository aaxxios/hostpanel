/**
 * Mount tests for AddClientView: it renders against the real kit, sends the form without a client
 * pre-check, and shows the API's own field messages under the fields.
 */
import { flushPromises } from '@vue/test-utils'
import { describe, expect, it } from 'vitest'
import AddClientView from '@/views/AddClientView.vue'
import { mountClientView } from '../helpers/clientViews'
import { callOf, invalid, stubFetch } from '../helpers/fetch'

describe('AddClientView', () => {
  it('mounts without throwing', async () => {
    const wrapper = await mountClientView(AddClientView)

    expect(wrapper.exists()).toBe(true)
  })

  it('renders its main action', async () => {
    const wrapper = await mountClientView(AddClientView)

    expect(wrapper.find('[data-testid="add-client-submit"]').exists()).toBe(true)
  })

  it('sends an empty form and shows the API field messages under the fields', async () => {
    const wrapper = await mountClientView(AddClientView)
    const mock = stubFetch(
      invalid('Validation failed', {
        Email: ['Email is required by the API'],
        FirstName: ['First name is required by the API'],
      }),
    )

    await wrapper.find('[data-testid="add-client-submit"]').trigger('click')
    await flushPromises()

    expect(callOf(mock).method).toBe('POST')
    expect(wrapper.text()).toContain('Email is required by the API')
    expect(wrapper.text()).toContain('First name is required by the API')
  })
})

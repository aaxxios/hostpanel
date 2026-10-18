/**
 * Mount tests for CwpIntegrationView: the header, the form and the live status card from the
 * server-info endpoint, and the refusal a failed test returns.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import CwpIntegrationView from '@/views/CwpIntegrationView.vue'
import { stubRoutes } from '../helpers/routes'
import { json } from '../helpers/fetch'
import { mountView } from '../helpers/mount'
import { integrationDetail } from '../helpers/integrationFixtures'

/** Live server info of a connected CWP server. */
const connectedInfo = { connected: true, accountsCount: 21, cwpVersion: '0.9.8', lastTestedAt: null, errorMessage: null }

describe('CwpIntegrationView', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('shows the header with the version and the status card with the accounts', async () => {
    stubRoutes({
      'GET /api/admin/integrations/cwp/server-info': () => json(connectedInfo),
      'GET /api/admin/integrations/cwp': () => json(integrationDetail({ slug: 'cwp', name: 'CWP' })),
    })
    const wrapper = await mountView(CwpIntegrationView)
    await flushPromises()

    expect(wrapper.get('[data-testid="cwp-version"]').text()).toBe('v0.9.8')
    expect(wrapper.get('[data-testid="cwp-status"]').text()).toBe('Connected')
    expect(wrapper.get('[data-testid="provider-status"]').text()).toContain('21')
    expect(wrapper.find('[data-testid="integration-config-form"]').exists()).toBe(true)
  })

  it('shows the message of a failed test under the form', async () => {
    stubRoutes({
      'GET /api/admin/integrations/cwp/server-info': () => json(connectedInfo),
      'GET /api/admin/integrations/cwp': () => json(integrationDetail({ slug: 'cwp', name: 'CWP' })),
      'POST /api/admin/integrations/cwp/test': () => json({ success: false, message: 'Bad token.' }),
    })
    const wrapper = await mountView(CwpIntegrationView)
    await flushPromises()

    await wrapper.get('[data-testid="provider-status-test"]').trigger('click')
    await flushPromises()

    expect(wrapper.get('[data-testid="cwp-test-error"]').text()).toContain('Bad token.')
  })
})

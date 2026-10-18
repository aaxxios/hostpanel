/**
 * Mount tests for IntegrationsView: the cards of the route's category, the active count, the
 * empty state and the API's refusal sentence.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import IntegrationsView from '@/views/IntegrationsView.vue'
import { stubRoutes } from '../helpers/routes'
import { json, problem } from '../helpers/fetch'
import { mountView } from '../helpers/mount'
import { integrationDto } from '../helpers/integrationFixtures'

describe('IntegrationsView', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('shows only the integrations of the route category, with the active count', async () => {
    stubRoutes({
      'GET /api/admin/integrations': () => json([
        integrationDto({ slug: 'stripe', name: 'Stripe', isEnabled: true }),
        integrationDto({ slug: 'paypal', name: 'PayPal', isEnabled: false }),
        integrationDto({ slug: 'smtp', name: 'SMTP', category: 'email' }),
      ]),
    })
    const wrapper = await mountView(IntegrationsView, { path: '/integrations/payments', pattern: '/integrations/:category?' })

    expect(wrapper.find('[data-testid="integration-card-stripe"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="integration-card-paypal"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="integration-card-smtp"]').exists()).toBe(false)
    expect(wrapper.text()).toContain('Integrations: 2')
    expect(wrapper.text()).toContain('1 active')
  })

  it('defaults to payments on the bare section', async () => {
    stubRoutes({ 'GET /api/admin/integrations': () => json([integrationDto()]) })
    const wrapper = await mountView(IntegrationsView, { path: '/integrations', pattern: '/integrations/:category?' })

    expect(wrapper.find('[data-testid="integration-card-stripe"]').exists()).toBe(true)
  })

  it('shows the empty state for a category with nothing in it', async () => {
    stubRoutes({ 'GET /api/admin/integrations': () => json([integrationDto()]) })
    const wrapper = await mountView(IntegrationsView, { path: '/integrations/fraud', pattern: '/integrations/:category?' })

    expect(wrapper.find('[data-testid="integrations-empty"]').exists()).toBe(true)
  })

  it('shows the API sentence when the load is refused', async () => {
    stubRoutes({ 'GET /api/admin/integrations': () => problem('Integrations are unavailable.', 503) })
    const wrapper = await mountView(IntegrationsView, { path: '/integrations/payments', pattern: '/integrations/:category?' })

    expect(wrapper.get('[data-testid="integrations-error"]').text()).toContain('Integrations are unavailable.')
  })
})

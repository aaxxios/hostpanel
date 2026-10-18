/**
 * Mount tests for IntegrationBreadcrumb: the link back to the list and the integration name.
 */
import { describe, expect, it } from 'vitest'
import IntegrationBreadcrumb from '@/components/integrations/IntegrationBreadcrumb.vue'
import { mountWithApp } from '../../helpers/mount'

describe('IntegrationBreadcrumb', () => {
  it('links back to the integrations list and shows the name', () => {
    const wrapper = mountWithApp(IntegrationBreadcrumb, { props: { name: 'Stripe' } })

    expect(wrapper.get('[data-testid="integration-breadcrumb-root"]').attributes('href')).toBe('/integrations')
    expect(wrapper.get('[data-testid="integration-breadcrumb-root"]').text()).toBe('Integrations')
    expect(wrapper.text()).toContain('Stripe')
  })
})

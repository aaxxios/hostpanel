/**
 * Mount tests for IntegrationCard: name, status badge, plugin badge and the link to the
 * configuration page.
 */
import { describe, expect, it } from 'vitest'
import IntegrationCard from '@/components/integrations/IntegrationCard.vue'
import { mountWithApp } from '../../helpers/mount'
import { integrationDto } from '../../helpers/integrationFixtures'

describe('IntegrationCard', () => {
  it('links to the configuration page and shows name and active status', () => {
    const wrapper = mountWithApp(IntegrationCard, { props: { integration: integrationDto() } })

    expect(wrapper.get('[data-testid="integration-card-stripe"]').attributes('href')).toBe('/integrations/stripe/configure')
    expect(wrapper.text()).toContain('Stripe')
    expect(wrapper.get('[data-testid="integration-status-stripe"]').text()).toBe('Active')
  })

  it('shows an inactive integration as inactive', () => {
    const wrapper = mountWithApp(IntegrationCard, { props: { integration: integrationDto({ isEnabled: false }) } })

    expect(wrapper.get('[data-testid="integration-status-stripe"]').text()).toBe('Inactive')
  })

  it('marks a plugin-provided integration', () => {
    const wrapper = mountWithApp(IntegrationCard, { props: { integration: integrationDto({ isPlugin: true }) } })

    expect(wrapper.text()).toContain('Plugin')
  })
})

/**
 * Fixtures for the integrations screens and the plugin manager: one payment integration, its
 * detail with a field list, and one installed plugin.
 */
import type { IntegrationDetailDto, IntegrationDto } from '../../src/types/integration'
import type { PluginListItemDto } from '../../src/types/plugin'

/** A disabled payment integration as the list endpoint returns it. */
export const stripe: IntegrationDto = {
  slug: 'stripe',
  name: 'Stripe',
  description: 'Card payments',
  category: 'payments',
  isEnabled: false,
  lastTestedAt: null,
}

/** The same integration with the fields its configuration form renders. */
export const stripeDetail: IntegrationDetailDto = {
  ...stripe,
  config: { secretKey: '', publishableKey: '' },
  fieldDefinitions: [
    { key: 'secretKey', label: 'Secret key', type: 'password', required: true },
    { key: 'publishableKey', label: 'Publishable key', type: 'text', required: false },
  ],
}

/** An installed, loaded provisioning plugin. */
export const cwpPlugin: PluginListItemDto = {
  id: 'innovayse-cwp',
  name: 'CWP provisioning',
  version: '1.0.0',
  author: 'Hostpanel',
  description: 'Provisions hosting accounts on CWP servers.',
  type: 'provisioning',
  category: 'Hosting / Provisioning',
  color: '#1a73e8',
  isLoaded: true,
}

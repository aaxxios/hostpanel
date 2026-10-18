/**
 * Fixtures of the integrations tests: a summary and a detail as the admin API returns them.
 */
import type { IntegrationDetailDto, IntegrationDto } from '@/types/integration'

/**
 * Builds an integration summary.
 *
 * @param overrides - Fields to change on the default summary.
 * @returns The summary.
 */
export const integrationDto = (overrides: Partial<IntegrationDto> = {}): IntegrationDto => ({
  slug: 'stripe',
  name: 'Stripe',
  description: 'Card payments',
  category: 'payments',
  isEnabled: true,
  lastTestedAt: null,
  ...overrides,
})

/**
 * Builds an integration detail with one text, one select, one textarea and one toggle field.
 *
 * @param overrides - Fields to change on the default detail.
 * @returns The detail.
 */
export const integrationDetail = (overrides: Partial<IntegrationDetailDto> = {}): IntegrationDetailDto => ({
  ...integrationDto(),
  config: { secretKey: 'sk_test', mode: 'Test', note: 'hello', live: 'false' },
  fieldDefinitions: [
    { key: 'secretKey', label: 'Secret key', type: 'password', required: true },
    { key: 'mode', label: 'Mode', type: 'select', required: false, options: ['Live', 'Test'] },
    { key: 'note', label: 'Note', type: 'textarea', required: false },
    { key: 'live', label: 'Live payments', type: 'toggle', required: false },
  ],
  ...overrides,
})

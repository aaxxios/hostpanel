/**
 * Unit tests for the integration value sets: the strings the API stores and the category names it
 * groups integrations under.
 */
import { describe, expect, it } from 'vitest'
import { IntegrationCategoryName, IntegrationEncryption, IntegrationMode, IntegrationYesNo } from '@/types/integration'
import { INTEGRATION_META } from '@/types/integrationmeta'

describe('integration value sets', () => {
  it('keep the strings the API stores', () => {
    expect(Object.values(IntegrationMode)).toEqual(['Live', 'Test', 'Sandbox'])
    expect(Object.values(IntegrationYesNo)).toEqual(['Yes', 'No'])
    expect(Object.values(IntegrationEncryption)).toEqual(['TLS', 'SSL', 'None'])
  })

  it('keep the category names the API groups integrations under', () => {
    expect(Object.values(IntegrationCategoryName)).toEqual([
      'Payment Gateways',
      'Domain Registrars',
      'Hosting / Provisioning',
      'Email / SMTP',
      'Fraud Protection',
    ])
  })

  it('offer the payment modes through the enum', () => {
    const modeField = INTEGRATION_META.stripe.fields.find((field) => field.key === 'mode')

    expect(modeField?.options).toEqual([IntegrationMode.Live, IntegrationMode.Test])
  })
})

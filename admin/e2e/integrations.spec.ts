/**
 * Integrations flow: a category page lists its integrations, a card opens the configuration form,
 * saving PUTs the enabled flag and the field values, and "test connection" posts the test.
 */
import { expect, test } from '@playwright/test'
import { stripe, stripeDetail } from './fixtures/integrations'
import { fieldOf } from './support/locators'
import { json, stubApi } from './support/stubApi'

test('the payments page lists its integrations and a card opens the configuration', async ({ page }) => {
  await stubApi(page, {
    signedIn: true,
    routes: [json('GET', '/api/admin/integrations', [stripe]), json('GET', `/api/admin/integrations/${stripe.slug}`, stripeDetail)],
  })

  await page.goto('/integrations/payments')
  await expect(page.getByTestId('integrations-grid')).toBeVisible()
  await expect(page.getByTestId(`integration-card-${stripe.slug}`)).toContainText(stripe.name)

  await page.getByTestId(`integration-card-${stripe.slug}`).click()

  await page.waitForURL(`**/integrations/${stripe.slug}/configure`)
  await expect(page.getByTestId('integration-config-form')).toBeVisible()
})

test('saving the configuration sends the enabled flag and the field values', async ({ page }) => {
  const api = await stubApi(page, {
    signedIn: true,
    routes: [
      json('GET', '/api/admin/integrations', [stripe]),
      json('GET', `/api/admin/integrations/${stripe.slug}`, stripeDetail),
      { method: 'PUT', path: `/api/admin/integrations/${stripe.slug}`, respond: () => ({ status: 204 }) },
    ],
  })

  await page.goto(`/integrations/${stripe.slug}/configure`)
  await expect(page.getByTestId('integration-config-form')).toBeVisible()

  await page.getByTestId('integration-enabled').click()
  await fieldOf(page, 'integration-field-secretKey').fill('sk_test_123')
  await fieldOf(page, 'integration-field-publishableKey').fill('pk_test_123')
  await page.getByTestId('integration-save').click()

  await expect.poll(() => api.callsTo('PUT', `/api/admin/integrations/${stripe.slug}`).length).toBe(1)
  expect(api.callsTo('PUT', `/api/admin/integrations/${stripe.slug}`)[0]?.body).toEqual({
    isEnabled: true,
    config: { secretKey: 'sk_test_123', publishableKey: 'pk_test_123' },
  })
})

test('testing the connection posts the test and a refusal shows the API sentence', async ({ page }) => {
  const sentence = 'The secret key was rejected by the provider.'
  const api = await stubApi(page, {
    signedIn: true,
    routes: [
      json('GET', '/api/admin/integrations', [stripe]),
      json('GET', `/api/admin/integrations/${stripe.slug}`, stripeDetail),
      json(
        'POST',
        `/api/admin/integrations/${stripe.slug}/test`,
        { type: 'UPSTREAM', title: 'Rejected', status: 502, detail: sentence },
        502,
      ),
    ],
  })

  await page.goto(`/integrations/${stripe.slug}/configure`)
  await page.getByTestId('integration-test').click()

  await expect.poll(() => api.callsTo('POST', `/api/admin/integrations/${stripe.slug}/test`).length).toBe(1)
  await expect(page.getByTestId('integration-status-result')).toContainText(sentence)
})

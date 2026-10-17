/**
 * Gateway log flow: the screen lists what the API returns and sends the debug filter on search.
 */
import { expect, test } from '@playwright/test'
import { gatewayLogPage } from './fixtures/gatewayLog'
import { json, stubApi } from './support/stubApi'

test('the gateway log lists the API entries and filters by debug text', async ({ page }) => {
  const api = await stubApi(page, {
    signedIn: true,
    routes: [json('GET', '/api/gateway-logs', gatewayLogPage)],
  })

  await page.goto('/billing/gateway-log')
  await expect(page.getByTestId('billing-gateway-log-table')).toContainText('Invoice #9 order ord-9')
  await expect(page.getByTestId('billing-gateway-log-table')).toContainText('Invoice #4 ref ref-4')
  await expect(page.getByTestId('billing-gateway-log-count')).toContainText('2 Records Found')

  await page.getByTestId('billing-gateway-log-filter-toggle').click()
  await page.getByTestId('billing-gateway-log-debug').fill('ref-4')
  await page.getByTestId('billing-gateway-log-search').click()

  await expect.poll(() => api.callsTo('GET', '/api/gateway-logs').length).toBeGreaterThan(1)
  expect(api.callsTo('GET', '/api/gateway-logs').at(-1)?.search).toContain('debugData=ref-4')
})

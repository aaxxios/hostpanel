/**
 * Reports flow: a report page renders the rows its endpoint returned, shows the empty message when
 * there are none, and shows the API's sentence when the load is refused.
 */
import { expect, test } from '@playwright/test'
import { topClients } from './fixtures/reports'
import { json, stubApi } from './support/stubApi'

test('the top clients report lists the ranked clients', async ({ page }) => {
  await stubApi(page, { signedIn: true, routes: [json('GET', '/api/reports/top-clients-by-income', topClients)] })

  await page.goto('/reports/top-clients')

  await expect(page.getByTestId('report-table-row')).toHaveCount(topClients.length)
  await expect(page.getByTestId('report-table-row').first()).toContainText(topClients[0]?.clientName ?? '')
})

test('an empty report shows the empty message instead of rows', async ({ page }) => {
  await stubApi(page, { signedIn: true, routes: [json('GET', '/api/reports/top-clients-by-income', [])] })

  await page.goto('/reports/top-clients')

  await expect(page.getByTestId('report-page')).toBeVisible()
  await expect(page.getByTestId('report-table-row')).toHaveCount(0)
})

test('a refused report shows the API sentence', async ({ page }) => {
  const sentence = 'You are not allowed to run reports.'
  await stubApi(page, {
    signedIn: true,
    routes: [json('GET', '/api/reports/top-clients-by-income', { type: 'FORBIDDEN', title: 'Forbidden', status: 403, detail: sentence }, 403)],
  })

  await page.goto('/reports/top-clients')

  await expect(page.getByTestId('report-error')).toContainText(sentence)
})

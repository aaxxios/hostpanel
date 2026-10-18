/**
 * Client detail flow: the summary page loads its rollups, and editing the profile saves the
 * changed field, re-reads the record and confirms the save.
 */
import { expect, test } from '@playwright/test'
import { adaDetail } from './fixtures/clients'
import { emptyClientSummary } from './fixtures/clientSummary'
import type { ClientDetail } from '../src/types/client'
import { fieldOf } from './support/locators'
import { json, stubApi } from './support/stubApi'

test('the summary page shows the client and its rollups', async ({ page }) => {
  await stubApi(page, {
    signedIn: true,
    routes: [
      json('GET', '/api/clients/7', adaDetail),
      json('GET', '/api/clients/7/summary', emptyClientSummary),
      json('GET', '/api/services', { items: [], totalCount: 0, page: 1, pageSize: 5 }),
      json('GET', '/api/domains', { items: [], totalCount: 0, page: 1, pageSize: 5 }),
      json('GET', '/api/billing/quotes/client/7', { items: [], totalCount: 0, page: 1, pageSize: 5 }),
    ],
  })

  await page.goto('/clients/7/summary')

  await expect(page.getByTestId('client-header-name')).toHaveText(`${adaDetail.firstName} ${adaDetail.lastName}`)
  await expect(page.getByTestId('client-summary-info')).toContainText(adaDetail.email)
  await expect(page.getByTestId('client-summary-billing')).toBeVisible()
})

test('editing the profile saves the new value and confirms it', async ({ page }) => {
  // The record as the "database" holds it; the PUT mutates it so the re-read returns the edit.
  const stored: ClientDetail = { ...adaDetail }

  const api = await stubApi(page, {
    signedIn: true,
    routes: [
      { method: 'GET', path: '/api/clients/7', respond: () => ({ body: stored }) },
      {
        method: 'PUT',
        path: '/api/clients/7',
        respond: (call) => {
          Object.assign(stored, call.body)
          return { status: 204 }
        },
      },
      json('GET', '/api/admin/currencies', []),
      json('GET', '/api/reference/countries', []),
    ],
  })

  await page.goto('/clients/7/profile')
  await expect(fieldOf(page, 'client-profile-first-name')).toHaveValue(adaDetail.firstName)

  await fieldOf(page, 'client-profile-first-name').fill('Augusta')
  await page.getByTestId('client-profile-save').click()

  await expect(page.getByTestId('client-profile-saved')).toBeVisible()
  await expect(page.getByTestId('client-header-name')).toHaveText(`Augusta ${adaDetail.lastName}`)
  expect(api.callsTo('PUT', '/api/clients/7')[0]?.body).toMatchObject({ firstName: 'Augusta', lastName: adaDetail.lastName })
})

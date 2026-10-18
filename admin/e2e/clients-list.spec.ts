/**
 * Clients list flow: the name filter is sent to the API as `search` and narrows the table, and
 * the pagination control asks the API for the page it was clicked to.
 */
import { expect, test } from '@playwright/test'
import { ada, clientsPage, grace } from './fixtures/clients'
import type { ClientListItem } from '../src/types/client'
import type { PagedResult } from '../src/types/pagedresult'
import { fieldOf } from './support/locators'
import { json, stubApi } from './support/stubApi'

test('searching by name sends the filter and shows only the matching client', async ({ page }) => {
  const api = await stubApi(page, {
    signedIn: true,
    routes: [
      {
        method: 'GET',
        path: '/api/clients',
        respond: (call) => {
          const term = new URLSearchParams(call.search).get('search')?.toLowerCase() ?? ''
          const items = clientsPage.items.filter((client) => `${client.firstName} ${client.lastName}`.toLowerCase().includes(term))
          return { body: { ...clientsPage, items, totalCount: items.length } }
        },
      },
    ],
  })

  await page.goto('/clients')
  await expect(page.getByTestId('client-row')).toHaveCount(2)

  await fieldOf(page, 'clients-list-filter-name').fill('Grace')
  await page.getByTestId('clients-list-search').click()

  await expect(page.getByTestId('client-row')).toHaveCount(1)
  await expect(page.getByTestId('client-row')).toContainText(grace.email)
  await expect(page.getByTestId('client-row')).not.toContainText(ada.email)
  expect(api.callsTo('GET', '/api/clients').at(-1)?.search).toContain('search=Grace')
})

test('the pagination control loads the page that was clicked', async ({ page }) => {
  const pageSize = 20
  const totalCount = 45

  /**
   * Builds one page of generated clients, as the list endpoint would return it.
   *
   * @param number - The 1-based page.
   * @returns The page body.
   */
  const pageOf = (number: number): PagedResult<ClientListItem> => ({
    items: Array.from({ length: Math.min(pageSize, totalCount - (number - 1) * pageSize) }, (_, index) => {
      const id = (number - 1) * pageSize + index + 1
      return { ...ada, id, userId: `user-${id}`, email: `client${id}@example.test`, firstName: 'Client', lastName: String(id) }
    }),
    totalCount,
    page: number,
    pageSize,
  })

  const api = await stubApi(page, {
    signedIn: true,
    routes: [
      {
        method: 'GET',
        path: '/api/clients',
        respond: (call) => ({ body: pageOf(Number(new URLSearchParams(call.search).get('page') ?? 1)) }),
      },
    ],
  })

  await page.goto('/clients')
  await expect(page.getByTestId('client-row')).toHaveCount(pageSize)

  await page.getByTestId('clients-list-pagination-page-2').click()

  await expect(page.getByTestId('client-row').first()).toContainText('client21@example.test')
  expect(api.callsTo('GET', '/api/clients').at(-1)?.search).toContain('page=2')
})

test('a refused load shows the API sentence', async ({ page }) => {
  const sentence = 'You are not allowed to list clients.'
  await stubApi(page, {
    signedIn: true,
    routes: [json('GET', '/api/clients', { type: 'FORBIDDEN', title: 'Forbidden', status: 403, detail: sentence }, 403)],
  })

  await page.goto('/clients')

  await expect(page.getByTestId('clients-list-error')).toContainText(sentence)
})

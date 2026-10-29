/**
 * Support content flows: publishing an announcement posts its title, body and flag and returns to
 * the list, and adding a knowledgebase category posts it and shows it in the category grid.
 */
import { HttpMethod } from '../src/types/http'
import { expect, test } from '@playwright/test'
import type { Announcement } from '../src/types/announcement'
import type { KbCategory } from '../src/types/kbarticle'
import { fieldOf } from './support/locators'
import { json, stubApi } from './support/stubApi'

test('publishing an announcement posts it and returns to the list', async ({ page }) => {
  // The announcements as the "database" holds them; the POST appends the new one.
  const stored: Announcement[] = []

  const api = await stubApi(page, {
    signedIn: true,
    routes: [
      {
        method: HttpMethod.Get,
        path: '/api/announcements',
        respond: () => ({ body: { items: stored, pageNumber: 1, pageSize: 20, totalCount: stored.length, totalPages: Math.ceil(stored.length / 20) } }),
      },
      {
        method: HttpMethod.Post,
        path: '/api/announcements',
        respond: (call) => {
          const sent = call.body as { title: string; content: string; isPublished: boolean }
          stored.push({ id: 1, createdAt: '2026-10-14T09:00:00Z', ...sent })
          return { body: 1 }
        },
      },
    ],
  })

  await page.goto('/support/announcements/new')
  await fieldOf(page, 'announcement-form-title').fill('Scheduled maintenance')
  await fieldOf(page, 'announcement-form-content').fill('The panel is read-only on Sunday night.')
  await page.getByTestId('announcement-form-published').check()
  await page.getByTestId('announcement-form-save').click()

  await page.waitForURL('**/support/announcements')
  await expect(page.getByTestId('announcements-row-1')).toContainText('Scheduled maintenance')
  expect(api.callsTo(HttpMethod.Post, '/api/announcements')[0]?.body).toEqual({
    title: 'Scheduled maintenance',
    content: 'The panel is read-only on Sunday night.',
    isPublished: true,
  })
})

test('adding a knowledgebase category posts it and shows it in the grid', async ({ page }) => {
  const stored: KbCategory[] = []

  const api = await stubApi(page, {
    signedIn: true,
    routes: [
      { method: HttpMethod.Get, path: '/api/admin/knowledgebase/categories', respond: () => ({ body: stored }) },
      json(HttpMethod.Get, '/api/admin/knowledgebase', []),
      {
        method: HttpMethod.Post,
        path: '/api/admin/knowledgebase/categories',
        respond: (call) => {
          const sent = call.body as { name: string; description: string; isHidden: boolean }
          stored.push({ id: 5, parentCategoryId: null, articleCount: 0, ...sent })
          return { body: 5 }
        },
      },
    ],
  })

  await page.goto('/support/knowledgebase')
  await expect(page.getByTestId('knowledgebase-category-add')).toBeDisabled()

  await fieldOf(page, 'knowledgebase-category-name').fill('Getting started')
  await fieldOf(page, 'knowledgebase-category-description').fill('First steps with the panel')
  await page.getByTestId('knowledgebase-category-add').click()

  await expect(page.getByTestId('knowledgebase-category-5')).toContainText('Getting started')
  expect(api.callsTo(HttpMethod.Post, '/api/admin/knowledgebase/categories')[0]?.body).toEqual({
    name: 'Getting started',
    description: 'First steps with the panel',
    isHidden: false,
  })
})

test('a refused announcement save shows the API sentence and stays on the form', async ({ page }) => {
  const sentence = 'An announcement with this title already exists.'
  await stubApi(page, {
    signedIn: true,
    routes: [json(HttpMethod.Post, '/api/announcements', { type: 'CONFLICT', title: 'Conflict', status: 409, detail: sentence }, 409)],
  })

  await page.goto('/support/announcements/new')
  await fieldOf(page, 'announcement-form-title').fill('Scheduled maintenance')
  await page.getByTestId('announcement-form-save').click()

  await expect(page.getByTestId('announcement-form-error')).toContainText(sentence)
  expect(new URL(page.url()).pathname).toBe('/support/announcements/new')
})

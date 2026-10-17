/**
 * Migration flow: with no jobs the screen offers to start one, creating a job posts the source
 * URL and shows the secret key to copy into the source plugin, and a refusal shows the API's
 * sentence inside the dialog flow.
 */
import { expect, test } from '@playwright/test'
import { pendingJob } from './fixtures/migration'
import type { MigrationJob } from '../src/types/migration'
import { fieldOf } from './support/locators'
import { json, stubApi } from './support/stubApi'

test('creating a migration posts the source and shows the secret key', async ({ page }) => {
  // The jobs as the server holds them; the POST creates the pending job.
  const stored: MigrationJob[] = []

  const api = await stubApi(page, {
    signedIn: true,
    routes: [
      { method: 'GET', path: '/api/admin/migrations', respond: () => ({ body: stored }) },
      {
        method: 'POST',
        path: '/api/admin/migrations',
        respond: () => {
          stored.push({ ...pendingJob })
          return { body: pendingJob }
        },
      },
      { method: 'GET', path: `/api/admin/migrations/${pendingJob.id}`, respond: () => ({ body: pendingJob }) },
    ],
  })

  await page.goto('/migration')
  await expect(page.getByTestId('migration-history-empty')).toBeVisible()

  await page.getByTestId('migration-new').click()
  await expect(page.getByTestId('migration-create')).toBeDisabled()
  await fieldOf(page, 'migration-source-url').fill(pendingJob.sourceUrl)
  await fieldOf(page, 'migration-label').fill(pendingJob.label ?? '')
  await page.getByTestId('migration-create').click()

  await expect(page.getByTestId('migration-secret-key')).toContainText(pendingJob.key)
  await expect(page.getByTestId(`migration-job-${pendingJob.id}`)).toBeVisible()
  const [created] = api.callsTo('POST', '/api/admin/migrations')
  expect(created?.body).toMatchObject({ sourceUrl: pendingJob.sourceUrl, label: pendingJob.label })
})

test('a refused creation shows the API sentence', async ({ page }) => {
  const sentence = 'The source URL is not reachable.'
  await stubApi(page, {
    signedIn: true,
    routes: [
      json('GET', '/api/admin/migrations', []),
      json('POST', '/api/admin/migrations', { type: 'UNREACHABLE', title: 'Unreachable', status: 400, detail: sentence }, 400),
    ],
  })

  await page.goto('/migration')
  await page.getByTestId('migration-new').click()
  await fieldOf(page, 'migration-source-url').fill('https://old.example.test')
  await page.getByTestId('migration-create').click()

  await expect(page.getByTestId('migration-error')).toContainText(sentence)
})

test('an existing job is listed in the history and selecting it shows its setup', async ({ page }) => {
  await stubApi(page, {
    signedIn: true,
    routes: [
      json('GET', '/api/admin/migrations', [pendingJob]),
      json('GET', `/api/admin/migrations/${pendingJob.id}`, pendingJob),
    ],
  })

  await page.goto('/migration')
  await expect(page.getByTestId(`migration-job-${pendingJob.id}`)).toBeVisible()

  await page.getByTestId(`migration-select-${pendingJob.id}`).click()

  await expect(page.getByTestId('migration-secret-key')).toContainText(pendingJob.key)
})

/**
 * Plugin manager flow: installed plugins are listed, uploading an archive installs it as
 * multipart data and shows the restart banner, restarting posts the command, and removing a
 * plugin deletes it from the list.
 */
import { expect, test } from '@playwright/test'
import { cwpPlugin } from './fixtures/integrations'
import { tinyPng } from './fixtures/settings'
import type { PluginListItemDto } from '../src/types/plugin'
import { json, stubApi } from './support/stubApi'

test('uploading an archive installs the plugin and offers the restart', async ({ page }) => {
  // The installed plugins as the server holds them; the install appends one.
  const stored: PluginListItemDto[] = []

  const api = await stubApi(page, {
    signedIn: true,
    routes: [
      { method: 'GET', path: '/api/admin/plugins', respond: () => ({ body: stored }) },
      {
        method: 'POST',
        path: '/api/admin/plugins/install',
        respond: () => {
          stored.push({ ...cwpPlugin, isLoaded: false })
          return { body: { requiresRestart: true } }
        },
      },
      { method: 'POST', path: '/api/admin/plugins/restart', respond: () => ({ status: 204 }) },
    ],
  })

  await page.goto('/plugins')
  await expect(page.getByTestId('plugins-empty')).toBeVisible()

  await page.getByTestId('plugin-zip-input').setInputFiles({ name: 'cwp.zip', mimeType: 'application/zip', buffer: tinyPng })
  await expect(page.getByTestId('plugin-selected-name')).toContainText('cwp.zip')
  await page.getByTestId('plugin-install').click()

  await expect(page.getByTestId(`plugin-card-${cwpPlugin.id}`)).toContainText(cwpPlugin.name)
  await expect(page.getByTestId('plugins-restart-banner')).toBeVisible()
  const [install] = api.callsTo('POST', '/api/admin/plugins/install')
  expect(install?.contentType).toContain('multipart/form-data')
  expect(install?.postData).toContain('filename="cwp.zip"')

  await page.getByTestId('plugins-restart').click()
  await expect.poll(() => api.callsTo('POST', '/api/admin/plugins/restart').length).toBe(1)
})

test('removing a plugin deletes it from the list', async ({ page }) => {
  const stored: PluginListItemDto[] = [{ ...cwpPlugin }]

  const api = await stubApi(page, {
    signedIn: true,
    routes: [
      { method: 'GET', path: '/api/admin/plugins', respond: () => ({ body: stored }) },
      {
        method: 'DELETE',
        path: `/api/admin/plugins/${cwpPlugin.id}`,
        respond: () => {
          stored.splice(0, stored.length)
          return { body: { requiresRestart: false } }
        },
      },
    ],
  })

  await page.goto('/plugins')
  await page.getByTestId(`plugin-remove-${cwpPlugin.id}`).click()

  await expect(page.getByTestId('plugins-empty')).toBeVisible()
  expect(api.callsTo('DELETE', `/api/admin/plugins/${cwpPlugin.id}`)).toHaveLength(1)
})

test('a refused install shows the API sentence', async ({ page }) => {
  const sentence = 'The archive does not contain a plugin manifest.'
  await stubApi(page, {
    signedIn: true,
    routes: [
      json('GET', '/api/admin/plugins', []),
      json('POST', '/api/admin/plugins/install', { type: 'INVALID_PLUGIN', title: 'Invalid plugin', status: 400, detail: sentence }, 400),
    ],
  })

  await page.goto('/plugins')
  await page.getByTestId('plugin-zip-input').setInputFiles({ name: 'bad.zip', mimeType: 'application/zip', buffer: tinyPng })
  await page.getByTestId('plugin-install').click()

  await expect(page.getByTestId('plugins-error')).toContainText(sentence)
})

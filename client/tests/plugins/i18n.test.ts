/**
 * Tests for `plugins/i18n.ts`: every app locale receives its merged message set, and a missing
 * locale file does not stop the others.
 *
 * @module tests/plugins/i18n.test
 */

import { beforeEach, describe, expect, it, vi } from 'vitest'
import { APP_LOCALES } from '~/constants/appLocales'

/** The plugin imports ~100 locale files through the module graph; on a busy runner that outlasts the 5 s default. */
const PLUGIN_TIMEOUT_MS = 60_000

/** The recorded `setLocaleMessage` calls. */
const setLocaleMessage = vi.fn()

/** The plugin function as Nuxt calls it. */
type I18nPlugin = (app: { $i18n: { setLocaleMessage: typeof setLocaleMessage } }) => Promise<void>

/**
 * Loads the plugin and runs it against the recording i18n instance.
 */
const runPlugin = async (): Promise<void> => {
  vi.stubGlobal('defineNuxtPlugin', (setup: unknown) => setup)
  const plugin = (await import('~/plugins/i18n')).default as unknown as I18nPlugin
  await plugin({ $i18n: { setLocaleMessage } })
}

beforeEach(() => {
  setLocaleMessage.mockReset()
  vi.resetModules()
})

describe('i18n plugin', () => {
  it('loads messages for every app locale', async () => {
    await runPlugin()

    expect(setLocaleMessage).toHaveBeenCalledTimes(APP_LOCALES.length)
    expect(setLocaleMessage.mock.calls.map(call => call[0])).toEqual([...APP_LOCALES])
  }, PLUGIN_TIMEOUT_MS)

  it('merges the module files into one non-empty message object per locale', async () => {
    await runPlugin()

    for (const call of setLocaleMessage.mock.calls) {
      expect(Object.keys(call[1] as Record<string, unknown>).length).toBeGreaterThan(0)
    }
  }, PLUGIN_TIMEOUT_MS)
})

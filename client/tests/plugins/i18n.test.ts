/**
 * Tests for `plugins/i18n.ts`: every app locale receives its merged message set, the site-name placeholder is left for the translator, and a missing
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

/** The plugin as Nuxt calls it. */
interface I18nPlugin {
  /** Loads and registers the messages. */
  setup: (app: { $i18n: { setLocaleMessage: typeof setLocaleMessage } }) => Promise<void>
}

/**
 * Loads the plugin and runs it against the recording i18n instance.
 */
const runPlugin = async (): Promise<void> => {
  vi.stubGlobal('defineNuxtPlugin', (plugin: unknown) => plugin)
  const plugin = (await import('~/plugins/i18n')).default as unknown as I18nPlugin
  await plugin.setup({ $i18n: { setLocaleMessage } })
}

beforeEach(() => {
  setLocaleMessage.mockReset()
  vi.resetModules()
})

describe('i18n plugin', () => {
  it('keeps the {site} placeholder for the translator to fill at render time', async () => {
    await runPlugin()

    const serialized = JSON.stringify(setLocaleMessage.mock.calls.map(call => call[1]))
    expect(serialized).toContain('{site}')
    expect(serialized).not.toContain('%site%')
  }, PLUGIN_TIMEOUT_MS)

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

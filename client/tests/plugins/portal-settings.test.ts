/**
 * Tests for `plugins/portal-settings.ts`: the settings are loaded once into shared state, kept
 * when already present, and fall back to an empty map when the request fails.
 *
 * @module tests/plugins/portal-settings.test
 */

import { ref } from 'vue'
import type { Ref } from 'vue'
import { beforeEach, describe, expect, it, vi } from 'vitest'

/** The plugin definition. */
interface PluginDefinition {
  /** Plugin name. */
  name: string
  /** The setup function. */
  setup: () => Promise<void>
}

/** The shared state the plugin fills. */
let state: Ref<Record<string, string>>

/** The stubbed `$fetch`. */
const fetchMock = vi.fn()

/**
 * Loads the plugin and runs its setup.
 *
 * @returns The plugin definition.
 */
const runPlugin = async (): Promise<PluginDefinition> => {
  vi.stubGlobal('defineNuxtPlugin', (definition: PluginDefinition) => definition)
  vi.stubGlobal('useState', () => state)
  vi.stubGlobal('$fetch', fetchMock)
  const plugin = (await import('~/plugins/portal-settings')).default as unknown as PluginDefinition
  await plugin.setup()
  return plugin
}

beforeEach(() => {
  state = ref<Record<string, string>>({})
  fetchMock.mockReset()
  vi.resetModules()
})

describe('portal-settings plugin', () => {
  it('is named so other plugins can depend on it', async () => {
    fetchMock.mockResolvedValue({})

    const plugin = await runPlugin()

    expect(plugin.name).toBe('portal-settings')
  })

  it('loads the public settings into the shared state', async () => {
    fetchMock.mockResolvedValue({ 'portal.template': 'nova' })

    await runPlugin()

    expect(fetchMock).toHaveBeenCalledWith('/api/portal/public/settings')
    expect(state.value).toEqual({ 'portal.template': 'nova' })
  })

  it('does not fetch again when the state already carries the settings', async () => {
    state.value = { 'portal.template': 'aurora' }

    await runPlugin()

    expect(fetchMock).not.toHaveBeenCalled()
  })

  it('falls back to an empty map when the request fails', async () => {
    fetchMock.mockRejectedValue(new Error('down'))

    await runPlugin()

    expect(state.value).toEqual({})
  })
})

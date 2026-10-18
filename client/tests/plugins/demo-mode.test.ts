/**
 * Tests for `plugins/demo-mode.ts`: the demo status is loaded through the demo store when the
 * plugin's setup runs, and the plugin is named so others can depend on it.
 *
 * @module tests/plugins/demo-mode.test
 */

import { beforeEach, describe, expect, it, vi } from 'vitest'

/** The stubbed store `load` action. */
const loadMock = vi.fn()

vi.mock('~/stores/demo', () => ({
  useDemoStore: () => ({ load: loadMock })
}))

/** The plugin definition. */
interface PluginDefinition {
  /** Plugin name. */
  name: string
  /** The setup function. */
  setup: () => Promise<void>
}

/**
 * Loads the plugin definition.
 *
 * @returns The plugin definition.
 */
const loadPlugin = async (): Promise<PluginDefinition> => {
  vi.stubGlobal('defineNuxtPlugin', (definition: PluginDefinition) => definition)
  return (await import('~/plugins/demo-mode')).default as unknown as PluginDefinition
}

beforeEach(() => {
  loadMock.mockReset()
  loadMock.mockResolvedValue(undefined)
  vi.resetModules()
})

describe('demo-mode plugin', () => {
  it('is named so other plugins can depend on it', async () => {
    const plugin = await loadPlugin()

    expect(plugin.name).toBe('demo-mode')
  })

  it('loads the demo status once on setup', async () => {
    const plugin = await loadPlugin()

    await plugin.setup()

    expect(loadMock).toHaveBeenCalledTimes(1)
  })
})

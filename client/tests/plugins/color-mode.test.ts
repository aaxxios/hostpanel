/**
 * Tests for `plugins/color-mode.ts`: the `<html>` class, the theme-color meta and the legacy
 * storage migration.
 *
 * @module tests/plugins/color-mode.test
 */

import { beforeEach, describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'
import type { Ref } from 'vue'
import { ColorMode } from '~/types/colormode'
import { ColorModeSystem } from '~/types/colormodesystem'

/** The head input the plugin registers. */
interface HeadInput {
  /** Attributes of `<html>`. */
  htmlAttrs: { class: () => string }
  /** Meta tags. */
  meta: Array<{ name: string, content: unknown, media?: string }>
}

/** The plugin definition. */
interface PluginDefinition {
  /** Plugin name. */
  name: string
  /** Plugins it waits for. */
  dependsOn: string[]
  /** The setup function. */
  setup: () => void
}

/** The mode on `<html>`. */
let applied: Ref<ColorMode | null>

/** The effective setting. */
let mode: Ref<string>

/** The stubbed `choose`. */
const choose = vi.fn()

/** The captured `useHead` call. */
const useHeadMock = vi.fn()

/** Operator settings the stubbed store answers with. */
let settings: Record<string, string> = {}

/**
 * Loads the plugin and runs its setup.
 *
 * @returns The plugin definition.
 */
const loadPlugin = async (): Promise<PluginDefinition> => {
  vi.stubGlobal('defineNuxtPlugin', (definition: PluginDefinition) => definition)
  vi.stubGlobal('useAppColorMode', () => ({ applied, mode, choose }))
  vi.stubGlobal('usePortalSettings', () => ({ get: (key: string) => settings[key] ?? '' }))
  vi.stubGlobal('useHead', useHeadMock)
  vi.stubGlobal('matchMedia', () => ({ addEventListener: vi.fn() }))
  const plugin = (await import('~/plugins/color-mode')).default as unknown as PluginDefinition
  plugin.setup()
  return plugin
}

beforeEach(() => {
  applied = ref<ColorMode | null>(ColorMode.Dark)
  mode = ref<string>(ColorMode.Dark)
  settings = {}
  choose.mockReset()
  useHeadMock.mockClear()
  localStorage.clear()
  vi.resetModules()
})

describe('color-mode plugin', () => {
  it('waits for the portal settings to load', async () => {
    const plugin = await loadPlugin()

    expect(plugin.name).toBe('color-mode')
    expect(plugin.dependsOn).toEqual(['portal-settings'])
  })

  it('writes the applied mode as the class on html', async () => {
    await loadPlugin()

    const head = useHeadMock.mock.calls[0]![0] as HeadInput
    expect(head.htmlAttrs.class()).toBe('dark')
  })

  it('writes the template page colour into theme-color', async () => {
    applied.value = ColorMode.Light
    settings['portal.template'] = 'nova'
    await loadPlugin()

    const head = useHeadMock.mock.calls[0]![0] as HeadInput
    const themeColors = head.meta.filter(tag => tag.name === 'theme-color')
    expect(themeColors).toHaveLength(2)
    // A chosen mode wins whatever the device scheme.
    expect(themeColors.map(tag => (tag.content as Ref<string>).value)).toEqual(['#f4f8fa', '#f4f8fa'])
  })

  it('gives each device scheme its own theme-color while no mode is chosen', async () => {
    settings['portal.template'] = 'nova'
    await loadPlugin()
    // The browser half reads a mode back from <html>; clear it to stand for the server render.
    applied.value = null

    const head = useHeadMock.mock.calls[0]![0] as HeadInput
    const byMedia = Object.fromEntries(head.meta
      .filter(tag => tag.name === 'theme-color')
      .map(tag => [tag.media, (tag.content as Ref<string>).value]))
    expect(byMedia).toEqual({ '(prefers-color-scheme: light)': '#f4f8fa', '(prefers-color-scheme: dark)': '#08191f' })
  })

  it('migrates a legacy stored choice and removes the key', async () => {
    localStorage.setItem('color-mode', 'light')

    await loadPlugin()

    expect(choose).toHaveBeenCalledWith(ColorMode.Light)
    expect(localStorage.getItem('color-mode')).toBeNull()
  })

  it('reads the class the bootstrap script set when nothing was applied yet', async () => {
    applied.value = null
    mode.value = ColorModeSystem.System
    document.documentElement.classList.add('light')

    await loadPlugin()

    expect(applied.value).toBe(ColorMode.Light)
    document.documentElement.classList.remove('light')
  })
})

/**
 * Tests for `plugins/motion.ts`: the motion plugin is installed on the Vue app.
 *
 * @module tests/plugins/motion.test
 */

import { MotionPlugin } from '@vueuse/motion'
import { describe, expect, it, vi } from 'vitest'

describe('motion plugin', () => {
  it('installs the motion plugin on the Vue app', async () => {
    vi.stubGlobal('defineNuxtPlugin', (setup: unknown) => setup)
    const use = vi.fn()
    /** The plugin as Nuxt calls it. */
    const plugin = (await import('~/plugins/motion')).default as unknown as (app: { vueApp: { use: typeof use } }) => void

    plugin({ vueApp: { use } })

    expect(use).toHaveBeenCalledWith(MotionPlugin)
  })
})

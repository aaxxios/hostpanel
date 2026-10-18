/**
 * Tests for the store-side translator: it asks the running Nuxt instance for its i18n plugin
 * and, with no instance to ask (a unit test, a call outside a render), hands back the key so
 * the caller still gets a non-empty string.
 */

import { afterEach, describe, expect, it, vi } from 'vitest'
import { translate } from '~/utils/translate'

describe('translate', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('returns the key when there is no Nuxt instance', () => {
    expect(translate('error.noResponse')).toBe('error.noResponse')
  })

  it('returns the key when the instance has no i18n plugin', () => {
    vi.stubGlobal('useNuxtApp', () => ({}))

    expect(translate('error.noResponse')).toBe('error.noResponse')
  })

  it('asks the i18n plugin, passing the named values through', () => {
    const t = vi.fn((key: string, named?: Record<string, unknown>) => `${key}:${JSON.stringify(named)}`)
    vi.stubGlobal('useNuxtApp', () => ({ $i18n: { t } }))

    expect(translate('client.setup.stepOf', { current: 1, total: 3 })).toBe('client.setup.stepOf:{"current":1,"total":3}')
    expect(t).toHaveBeenCalledWith('client.setup.stepOf', { current: 1, total: 3 })
  })
})

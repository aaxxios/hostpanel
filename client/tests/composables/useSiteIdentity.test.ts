/**
 * Tests for `useSiteIdentity`: the operator's site name and tagline, with the brand as fallback.
 *
 * @module tests/composables/useSiteIdentity.test
 */

import { afterEach, describe, expect, it, vi } from 'vitest'
import { useSiteIdentity } from '~/composables/useSiteIdentity'

/**
 * Stubs the settings reader with a fixed set of values.
 *
 * @param values Setting key to value.
 */
const stubSettings = (values: Record<string, string>): void => {
  vi.stubGlobal('usePortalSettings', () => ({ get: (key: string) => values[key] ?? '' }))
}

describe('useSiteIdentity', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('uses the name and tagline the operator set', () => {
    stubSettings({ 'portal.site.name': 'Acme Hosting', 'portal.site.tagline': 'Fast and calm' })

    const { name, tagline } = useSiteIdentity()

    expect(name.value).toBe('Acme Hosting')
    expect(tagline.value).toBe('Fast and calm')
  })

  it('falls back to the neutral product name and an empty tagline', () => {
    stubSettings({})

    const { name, tagline } = useSiteIdentity()

    expect(name.value).toBe('Hostpanel')
    expect(tagline.value).toBe('')
  })
})

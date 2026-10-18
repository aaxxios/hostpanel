/**
 * Tests for `useBrandLogo`: the light/dark/mark rule over the operator's uploaded images.
 *
 * @module tests/composables/useBrandLogo.test
 */

import { afterEach, describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'
import { useBrandLogo } from '~/composables/useBrandLogo'

/**
 * Stubs the settings reader, the colour mode and the site identity.
 *
 * @param settings Setting key to value.
 * @param isDark Whether the page is in dark mode.
 */
const stubRuntime = (settings: Record<string, string>, isDark = true): void => {
  vi.stubGlobal('usePortalSettings', () => ({ get: (key: string) => settings[key] ?? '' }))
  vi.stubGlobal('useAppColorMode', () => ({ isDark: ref(isDark) }))
  vi.stubGlobal('useSiteIdentity', () => ({ name: ref('Acme'), tagline: ref('') }))
}

describe('useBrandLogo', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('reports nothing custom when no logo was uploaded', () => {
    stubRuntime({})

    const logo = useBrandLogo()

    expect(logo.isCustom.value).toBe(false)
    expect(logo.light.value).toBe('')
    expect(logo.dark.value).toBe('')
    expect(logo.mark.value).toBe('')
    expect(logo.hasDarkVariant.value).toBe(false)
  })

  it('uses the site name as alt text', () => {
    stubRuntime({})

    expect(useBrandLogo().alt.value).toBe('Acme')
  })

  it('shows a single logo in both modes when only the light one exists', () => {
    stubRuntime({ 'portal.logo': '/l.png' })

    const logo = useBrandLogo()

    expect(logo.isCustom.value).toBe(true)
    expect(logo.light.value).toBe('/l.png')
    expect(logo.dark.value).toBe('/l.png')
    expect(logo.hasDarkVariant.value).toBe(false)
  })

  it('exposes both wordmarks when they differ', () => {
    stubRuntime({ 'portal.logo': '/l.png', 'portal.logo.dark': '/d.png' })

    const logo = useBrandLogo()

    expect(logo.hasDarkVariant.value).toBe(true)
    expect(logo.logoOnDark.value).toBe('/d.png')
  })

  it('picks the mode-aware logo for the current mode', () => {
    stubRuntime({ 'portal.logo': '/l.png', 'portal.logo.dark': '/d.png' }, true)
    expect(useBrandLogo().logo.value).toBe('/d.png')

    stubRuntime({ 'portal.logo': '/l.png', 'portal.logo.dark': '/d.png' }, false)
    expect(useBrandLogo().logo.value).toBe('/l.png')
  })

  it('prefers the uploaded mark and falls back to the wordmark', () => {
    stubRuntime({ 'portal.logo': '/l.png', 'portal.logo.mark': '/m.png' })
    expect(useBrandLogo().mark.value).toBe('/m.png')

    stubRuntime({ 'portal.logo': '/l.png' }, false)
    expect(useBrandLogo().mark.value).toBe('/l.png')
  })
})

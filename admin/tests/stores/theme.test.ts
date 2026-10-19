/**
 * Unit tests for the theme store: reading the stored choice, persisting a new one, following the
 * system scheme while on "system", and tolerating storage that throws.
 */
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { THEME_STORAGE_KEY } from '@/lib/themeBoot'
import { useThemeStore } from '@/stores/theme'
import { AdminTheme } from '@/types/admintheme'
import { resetThemeDocument, stubSchemeQuery } from '../helpers/theme'

describe('theme store', () => {
  beforeEach(() => {
    resetThemeDocument()
    setActivePinia(createPinia())
  })

  afterEach(() => {
    vi.unstubAllGlobals()
    vi.restoreAllMocks()
  })

  it('starts on "system" with nothing stored and resolves it from the system', () => {
    stubSchemeQuery(true)
    const store = useThemeStore()

    store.init()

    expect(store.choice).toBe(AdminTheme.System)
    expect(store.isLight).toBe(true)
    expect(document.documentElement.classList.contains('light')).toBe(true)
  })

  it('reads a stored choice', () => {
    stubSchemeQuery(true)
    window.localStorage.setItem(THEME_STORAGE_KEY, AdminTheme.Dark)
    const store = useThemeStore()

    store.init()

    expect(store.choice).toBe(AdminTheme.Dark)
    expect(store.isLight).toBe(false)
  })

  it('stores and applies a new choice', () => {
    stubSchemeQuery(false)
    const store = useThemeStore()
    store.init()

    store.setTheme(AdminTheme.Light)

    expect(window.localStorage.getItem(THEME_STORAGE_KEY)).toBe(AdminTheme.Light)
    expect(store.isLight).toBe(true)
    expect(document.documentElement.classList.contains('light')).toBe(true)
  })

  it('follows a system scheme change on "system" but not on an explicit choice', () => {
    const scheme = stubSchemeQuery(false)
    const store = useThemeStore()
    store.init()

    scheme.setLight(true)
    expect(store.isLight).toBe(true)

    store.setTheme(AdminTheme.Dark)
    scheme.setLight(true)
    expect(store.isLight).toBe(false)
  })

  it('adds the system listener only once', () => {
    const addEventListener = vi.fn()
    vi.stubGlobal('matchMedia', () => ({ matches: false, addEventListener }))
    const store = useThemeStore()

    store.init()
    store.init()

    expect(addEventListener).toHaveBeenCalledTimes(1)
  })

  it('still applies a choice when storage throws', () => {
    stubSchemeQuery(false)
    vi.spyOn(Storage.prototype, 'getItem').mockImplementation(() => {
      throw new Error('blocked')
    })
    vi.spyOn(Storage.prototype, 'setItem').mockImplementation(() => {
      throw new Error('blocked')
    })
    const store = useThemeStore()

    expect(store.choice).toBe(AdminTheme.System)
    store.setTheme(AdminTheme.Light)

    expect(store.choice).toBe(AdminTheme.Light)
  })
})

/**
 * Unit tests for `useCookieConsent`: the stored cookie is narrowed to the two known choices and
 * `accept` writes the chosen one back.
 *
 * @module tests/composables/useCookieConsent.test
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'
import { CookieConsent } from '~/types/cookieconsent'
import { COOKIE_CONSENT_NAME, useCookieConsent } from '~/composables/useCookieConsent'

/** The cookie ref `useCookie` hands back, replaced before each test. */
const cookie = ref<string | null>(null)

/** Spy for `useCookie`, to read the name and options the composable asked for. */
const useCookie = vi.fn(() => cookie)

describe('useCookieConsent', () => {
  beforeEach(() => {
    cookie.value = null
    useCookie.mockClear()
    vi.stubGlobal('useCookie', useCookie)
  })

  it('reads the consent cookie by its name', () => {
    useCookieConsent()

    expect(useCookie).toHaveBeenCalledWith(COOKIE_CONSENT_NAME, expect.objectContaining({ sameSite: 'lax' }))
  })

  it('reports no choice while the cookie is empty', () => {
    expect(useCookieConsent().consent.value).toBeNull()
  })

  it.each([CookieConsent.All, CookieConsent.Essential])('reports %s when the cookie holds it', (level) => {
    cookie.value = level

    expect(useCookieConsent().consent.value).toBe(level)
  })

  it('treats an unknown cookie value as no choice', () => {
    cookie.value = 'everything'

    expect(useCookieConsent().consent.value).toBeNull()
  })

  it('writes the choice to the cookie', () => {
    const { consent, accept } = useCookieConsent()

    accept(CookieConsent.Essential)

    expect(cookie.value).toBe('essential')
    expect(consent.value).toBe(CookieConsent.Essential)
  })
})

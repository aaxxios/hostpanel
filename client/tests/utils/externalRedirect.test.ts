/**
 * Tests for `utils/externalRedirect`: only a secure absolute address (and an insecure one in
 * development) may be followed by the browser.
 *
 * @module tests/utils/externalRedirect.test
 */

import { afterEach, describe, expect, it, vi } from 'vitest'
import { isSafeRedirectUrl, redirectToExternal } from '~/utils/externalRedirect'

describe('isSafeRedirectUrl', () => {
  it('accepts an absolute https address', () => {
    expect(isSafeRedirectUrl('https://bank.example/pay?id=1')).toBe(true)
  })

  it('refuses http outside development and accepts it in development', () => {
    expect(isSafeRedirectUrl('http://bank.example/pay')).toBe(false)
    expect(isSafeRedirectUrl('http://localhost:9000/pay', true)).toBe(true)
  })

  it('refuses script, data, relative, protocol-relative and malformed values in every mode', () => {
    for (const target of ['javascript:alert(1)', 'data:text/html,x', '/payment/result', '//bank.example/pay', 'not a url', '']) {
      expect(isSafeRedirectUrl(target, true)).toBe(false)
    }
  })
})

describe('redirectToExternal', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('hands the browser to a safe address', () => {
    const location = { href: '' }
    vi.stubGlobal('window', { location })

    expect(redirectToExternal('https://bank.example/pay')).toBe(true)
    expect(location.href).toBe('https://bank.example/pay')
  })

  it('does nothing for an unsafe address', () => {
    const location = { href: 'https://app.example/' }
    vi.stubGlobal('window', { location })

    expect(redirectToExternal('javascript:alert(1)')).toBe(false)
    expect(location.href).toBe('https://app.example/')
  })
})

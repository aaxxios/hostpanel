/**
 * Tests for `utils/returnPath.ts`: which return paths are followed after signing in, and the
 * sign-in URL that carries one.
 *
 * @module tests/utils/returnPath.test
 */

import { describe, expect, it } from 'vitest'
import { isInternalPath, isPrivateClientPath, isPublicClientPath, loginPathWithReturn, safeReturnPath } from '~/utils/returnPath'

describe('isPublicClientPath', () => {
  it('is true for the public pages and what is below them, ignoring query and hash', () => {
    expect(isPublicClientPath('/client/login')).toBe(true)
    expect(isPublicClientPath('/client/login?redirect=%2Fclient%2Finvoices')).toBe(true)
    expect(isPublicClientPath('/client/reset-password/abc')).toBe(true)
  })

  it.each(['/ru/client/login', '/hy/client/register', '/ru/client/reset-password/abc?token=1'])(
    'is true for the localized form %s',
    (path) => {
      expect(isPublicClientPath(path)).toBe(true)
    }
  )

  it('is false for a localized page that needs a session', () => {
    expect(isPublicClientPath('/ru/client/invoices')).toBe(false)
    expect(isPublicClientPath('/ru')).toBe(false)
    expect(isPublicClientPath('/en/client/login')).toBe(false)
  })

  it('is false for pages that need a session', () => {
    expect(isPublicClientPath('/client/invoices')).toBe(false)
    expect(isPublicClientPath('/client/loginx')).toBe(false)
  })
})

describe('isPrivateClientPath', () => {
  it.each(['/client/dashboard', '/client/invoices/7?tab=pay', '/ru/client/account', '/client'])(
    'is true for the signed-in page %s',
    (path) => {
      expect(isPrivateClientPath(path)).toBe(true)
    }
  )

  it.each(['/', '/hosting', '/domains/transfer', '/cart', '/ru/faq', '/client/login', '/hy/client/register', '/clientx/a'])(
    'is false for the open or public page %s',
    (path) => {
      expect(isPrivateClientPath(path)).toBe(false)
    }
  )
})

describe('isInternalPath', () => {
  it('accepts a path inside the app', () => {
    expect(isInternalPath('/client/invoices/7?tab=pay#top')).toBe(true)
  })

  it.each([
    ['an absolute URL', 'https://evil.example/x'],
    ['a protocol-relative URL', '//evil.example/x'],
    ['a backslash form', '/\\evil.example'],
    ['a control character', '/client/\nHost: x'],
    ['no leading slash', 'client/invoices'],
    ['an empty string', ''],
    ['an array', ['/client/invoices']],
    ['nothing', undefined]
  ])('rejects %s', (_label, value) => {
    expect(isInternalPath(value)).toBe(false)
  })
})

describe('safeReturnPath', () => {
  it('returns an internal path unchanged', () => {
    expect(safeReturnPath('/client/invoices/7')).toBe('/client/invoices/7')
  })

  it('falls back to the dashboard for an unsafe or missing value', () => {
    expect(safeReturnPath('https://evil.example')).toBe('/client/dashboard')
    expect(safeReturnPath(undefined)).toBe('/client/dashboard')
  })

  it('does not return to a public page, which would loop', () => {
    expect(safeReturnPath('/client/login?redirect=%2Fx')).toBe('/client/dashboard')
  })

  it('uses the fallback it is given', () => {
    expect(safeReturnPath(null, '/client/home')).toBe('/client/home')
  })
})

describe('loginPathWithReturn', () => {
  it('encodes the page to return to', () => {
    expect(loginPathWithReturn('/client/invoices/7?x=1')).toBe('/client/login?redirect=%2Fclient%2Finvoices%2F7%3Fx%3D1')
  })

  it('is the bare sign-in path when already on a public page', () => {
    expect(loginPathWithReturn('/client/login')).toBe('/client/login')
  })

  it('is the bare sign-in path for a value that is not an internal path', () => {
    expect(loginPathWithReturn('https://evil.example')).toBe('/client/login')
  })
})

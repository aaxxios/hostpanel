/**
 * Tests for the post-sign-in redirect validator: only same-app relative paths pass, and the login
 * location builder carries them in the `redirect` query.
 */
import { describe, expect, it } from 'vitest'
import { loginLocationFor, safeRedirectTarget } from '@/lib/redirectTarget'

describe('safeRedirectTarget', () => {
  it.each([
    '/clients/1/summary',
    '/clients?page=2&q=a%20b',
    '/billing/invoices#top',
    '/dashboard',
  ])('accepts the in-app path %s', (value) => {
    expect(safeRedirectTarget(value)).toBe(value)
  })

  it.each([
    ['protocol-relative', '//evil.example/x'],
    ['absolute http', 'http://evil.example/x'],
    ['absolute https', 'https://evil.example'],
    ['javascript scheme', 'javascript:alert(1)'],
    ['data scheme', 'data:text/html,x'],
    ['no leading slash', 'clients/1'],
    ['backslash trick', '/\\evil.example'],
    ['leading backslash', '\\/evil.example'],
    ['tab inside', '/\t/evil.example'],
    ['newline inside', '/a\nb'],
    ['empty', ''],
    ['login screen', '/login'],
    ['login with query', '/login?redirect=%2Fclients'],
    ['setup screen', '/setup'],
  ])('rejects %s', (_name, value) => {
    expect(safeRedirectTarget(value)).toBeNull()
  })

  it('rejects values that are not strings', () => {
    expect(safeRedirectTarget(undefined)).toBeNull()
    expect(safeRedirectTarget(null)).toBeNull()
    expect(safeRedirectTarget(['/clients'])).toBeNull()
    expect(safeRedirectTarget(42)).toBeNull()
  })
})

describe('loginLocationFor', () => {
  it('carries the requested full path, encoded, in the redirect query', () => {
    expect(loginLocationFor('/clients/1/summary?tab=x')).toBe('/login?redirect=%2Fclients%2F1%2Fsummary%3Ftab%3Dx')
  })

  it('falls back to the bare login path for an unacceptable target', () => {
    expect(loginLocationFor('//evil.example')).toBe('/login')
    expect(loginLocationFor('/login')).toBe('/login')
  })
})

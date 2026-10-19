import { describe, expect, it } from 'vitest'
import { isLauncherEnabled, resolveAppUrl, withAccountHandoff } from '~/composables/usePortalApps'

describe('isLauncherEnabled', () => {
  it('is off for a stock install, so no operator inherits links into nothing', () => {
    expect(isLauncherEnabled('', '')).toBe(false)
    expect(isLauncherEnabled('', undefined)).toBe(false)
    expect(isLauncherEnabled('', null)).toBe(false)
  })

  it('accepts the boolean Nuxt produces from NUXT_PUBLIC_PORTAL_APPS_ENABLED', () => {
    expect(isLauncherEnabled('', true)).toBe(true)
    expect(isLauncherEnabled('', false)).toBe(false)
  })

  it('accepts the text an admin setting stores', () => {
    for (const value of ['true', 'TRUE', '1', 'yes', 'on', ' true ']) {
      expect(isLauncherEnabled(value, '')).toBe(true)
    }
    for (const value of ['false', '0', 'no', 'off']) {
      expect(isLauncherEnabled(value, '')).toBe(false)
    }
  })

  it('lets the admin setting switch the launcher off against the environment', () => {
    expect(isLauncherEnabled('false', true)).toBe(false)
    expect(isLauncherEnabled('true', false)).toBe(true)
  })
})

describe('resolveAppUrl', () => {
  it('reports no URL when the deployment has none, so the app is dropped', () => {
    expect(resolveAppUrl('', '')).toBe('')
    expect(resolveAppUrl('', '', '/account')).toBe('')
  })

  it('appends the app path to the environment host', () => {
    expect(resolveAppUrl('', 'https://example.com', '/account')).toBe('https://example.com/account')
    expect(resolveAppUrl('', 'https://example.com/', '/account')).toBe('https://example.com/account')
    expect(resolveAppUrl('', 'https://tasks.example.com')).toBe('https://tasks.example.com')
  })

  it('takes an operator override as a complete URL, path and all', () => {
    expect(resolveAppUrl('https://my.host/portal', 'https://example.com', '/account'))
      .toBe('https://my.host/portal')
  })
})

describe('withAccountHandoff', () => {
  it('names the signed-in account on a cross-origin app URL', () => {
    expect(withAccountHandoff('https://sheets.example.test', 'a@example.test'))
      .toBe('https://sheets.example.test/?authuser=a%40example.test')
  })

  it('replaces rather than repeats an authuser already there', () => {
    expect(withAccountHandoff('https://docs.example.test/?authuser=old@x.com', 'new@x.com'))
      .toBe('https://docs.example.test/?authuser=new%40x.com')
  })

  it('leaves the URL alone with nobody signed in, or when it is not an absolute URL', () => {
    expect(withAccountHandoff('https://sheets.example.test', undefined)).toBe('https://sheets.example.test')
    expect(withAccountHandoff('/client', 'a@example.test')).toBe('/client')
    expect(withAccountHandoff('not a url', 'a@example.test')).toBe('not a url')
  })
})

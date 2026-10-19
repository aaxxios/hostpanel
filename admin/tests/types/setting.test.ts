/**
 * Unit tests for the setting keys: every key has an environment fallback name, and the toggle
 * strings are the ones the API stores.
 */
import { describe, expect, it } from 'vitest'
import { SettingEnv, SettingKey, SettingToggle } from '@/types/setting'

describe('SettingKey', () => {
  it('prefixes every key with portal', () => {
    for (const key of Object.values(SettingKey)) {
      expect(key.startsWith('portal.')).toBe(true)
    }
  })

  it('holds no key twice', () => {
    const keys = Object.values(SettingKey)

    expect(new Set(keys).size).toBe(keys.length)
  })
})

describe('SettingEnv', () => {
  it('names an environment variable for every key', () => {
    for (const key of Object.values(SettingKey)) {
      expect(SettingEnv[key]).toMatch(/^NUXT_PUBLIC_PORTAL_[A-Z_]+$/)
    }
  })

  it('names the tagline by its own variable, not the site name', () => {
    expect(SettingEnv[SettingKey.SiteTagline]).toBe('NUXT_PUBLIC_PORTAL_SITE_TAGLINE')
    expect(SettingEnv[SettingKey.ChatWebsiteTokenHy]).toBe('NUXT_PUBLIC_PORTAL_CHAT_WEBSITE_TOKEN_HY')
  })
})

describe('SettingToggle', () => {
  it('stores a switch as the strings true and false', () => {
    expect(SettingToggle.On).toBe('true')
    expect(SettingToggle.Off).toBe('false')
  })
})

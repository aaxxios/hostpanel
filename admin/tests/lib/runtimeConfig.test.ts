import { afterEach, describe, expect, it } from 'vitest'
import { getRuntimeConfig, normalizeBasePath, publicAssetUrl } from '@/lib/runtimeConfig'

/** Runtime config reader: normalisation, absent config and asset URLs. */
describe('runtimeConfig', () => {
  afterEach(() => {
    delete window.__APP_CONFIG__
  })

  it('normalises empty, slashless and trailing-slash paths', () => {
    expect(normalizeBasePath(undefined)).toBe('/')
    expect(normalizeBasePath('')).toBe('/')
    expect(normalizeBasePath('/')).toBe('/')
    expect(normalizeBasePath('admin')).toBe('/admin')
    expect(normalizeBasePath('/admin/')).toBe('/admin')
  })

  it('falls back to the root base when config.js is absent', () => {
    expect(getRuntimeConfig().basePath).toBe('/')
  })

  it('reads the base path injected by config.js', () => {
    window.__APP_CONFIG__ = { basePath: '/admin/' }
    expect(getRuntimeConfig().basePath).toBe('/admin')
  })

  it('builds public asset URLs under the base path', () => {
    expect(publicAssetUrl('integrations/a.svg')).toBe('/integrations/a.svg')
    window.__APP_CONFIG__ = { basePath: '/admin' }
    expect(publicAssetUrl('integrations/a.svg')).toBe('/admin/integrations/a.svg')
  })
})

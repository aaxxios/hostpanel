/**
 * Tests for `usePortalSettings`: an admin-managed setting wins over the environment value, which
 * wins over the caller's default.
 *
 * @module tests/composables/usePortalSettings.test
 */

import { afterEach, describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'
import { usePortalSettings } from '~/composables/usePortalSettings'

/**
 * Stubs the shared state and the runtime config.
 *
 * @param settings The operator-managed settings.
 * @param env The public runtime config values.
 */
const stubSources = (settings: Record<string, string>, env: Record<string, unknown>): void => {
  vi.stubGlobal('useState', () => ref(settings))
  vi.stubGlobal('useRuntimeConfig', () => ({ public: env }))
}

describe('usePortalSettings', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('prefers the admin setting, trimmed', () => {
    stubSources({ 'portal.template': '  nova ' }, { portalTemplate: 'aurora' })

    expect(usePortalSettings().get('portal.template', 'portalTemplate')).toBe('nova')
  })

  it('falls back to the environment value', () => {
    stubSources({}, { portalTemplate: ' aurora ' })

    expect(usePortalSettings().get('portal.template', 'portalTemplate')).toBe('aurora')
  })

  it('falls back to the caller default, then to an empty string', () => {
    stubSources({}, {})

    expect(usePortalSettings().get('portal.template', 'portalTemplate', 'classic')).toBe('classic')
    expect(usePortalSettings().get('portal.template', 'portalTemplate')).toBe('')
  })
})

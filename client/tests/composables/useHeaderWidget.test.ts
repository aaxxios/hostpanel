/**
 * Tests for `composables/useHeaderWidget.ts`: the runtime configuration decides whether a header
 * mounts the platform widget or draws its own account menu.
 *
 * @module tests/composables/useHeaderWidget.test
 */

import { describe, expect, it, vi } from 'vitest'
import { useHeaderWidget } from '~/composables/useHeaderWidget'

describe('useHeaderWidget', () => {
  it('is enabled for an sso deployment with a portal address', () => {
    vi.stubGlobal('useRuntimeConfig', () => ({ public: { authMode: 'sso', mainUrl: 'https://main.example.test' } }))

    expect(useHeaderWidget().enabled.value).toBe(true)
  })

  it('is disabled on a standalone or demo deployment', () => {
    vi.stubGlobal('useRuntimeConfig', () => ({ public: { authMode: 'local', mainUrl: '' } }))

    expect(useHeaderWidget().enabled.value).toBe(false)
  })
})

/**
 * Unit tests for nova's feature visibility rule: a card without a required setting always shows, a
 * card that names one shows only once the operator has switched it on.
 *
 * @module tests/templates/nova/features.test
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'
import { usePortalSettings } from '~/composables/usePortalSettings'
import { useVisibleFeatures } from '~/templates/nova/features'
import type { Feature } from '~/templates/nova/types/feature'

/**
 * Stubs the Nuxt auto-imports the rule reads, with the real settings reader on top of them.
 *
 * @param settings The operator-managed settings.
 * @param env The public runtime config values.
 */
const stubSources = (settings: Record<string, string>, env: Record<string, unknown> = {}): void => {
  vi.stubGlobal('useState', () => ref(settings))
  vi.stubGlobal('useRuntimeConfig', () => ({ public: env }))
  vi.stubGlobal('usePortalSettings', usePortalSettings)
}

/**
 * Builds a feature card.
 *
 * @param key Stable key.
 * @param requiresSetting Optional setting key.
 * @returns The card.
 */
const card = (key: string, requiresSetting?: string): Feature => ({
  key,
  icon: 'lucide:shield-check',
  titleKey: `t.${key}`,
  requiresSetting,
})

describe('useVisibleFeatures', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('always shows a card that requires no setting', () => {
    stubSources({})

    const visible = useVisibleFeatures([card('a'), card('b')])

    expect(visible.value.map((feature) => feature.key)).toEqual(['a', 'b'])
  })

  it('hides a card whose setting is empty or false', () => {
    stubSources({ 'portal.claims.cdn': 'false' })

    const visible = useVisibleFeatures([card('a'), card('nvme', 'portal.claims.nvme')])

    expect(visible.value.map((feature) => feature.key)).toEqual(['a'])
  })

  it('shows a card once the operator setting is true, keeping the original order', () => {
    stubSources({ 'portal.claims.nvme': 'true' })

    const visible = useVisibleFeatures([card('a'), card('nvme', 'portal.claims.nvme'), card('c')])

    expect(visible.value.map((feature) => feature.key)).toEqual(['a', 'nvme', 'c'])
  })

  it('ignores a runtime-config value: the setting is the only source', () => {
    stubSources({}, { claimsCdn: 'true' })

    const visible = useVisibleFeatures([card('cdn', 'portal.claims.cdn')])

    expect(visible.value).toEqual([])
  })
})

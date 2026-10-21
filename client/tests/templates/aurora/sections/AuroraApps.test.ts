/**
 * Unit tests for AuroraApps: a card per installer category, the count line only when a count is set.
 */
import { describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import AuroraApps from '~/templates/aurora/sections/AuroraApps.vue'
import { InstallerAppCategory } from '~/constants/installerAppCategory'
import { stubAuroraGlobals } from '../auroraGlobals'
import { balancedColumns } from '~/utils/balancedGrid'

/** What the mocked installer composable reports. */
const installer = vi.hoisted(() => ({ value: { count: '', categories: [] as InstallerAppCategory[] } }))

vi.mock('~/composables/useInstallerApps', () => ({
  /**
   * Stands in for the installer composable.
   *
   * @returns The mocked count and categories as a ref.
   */
  useInstallerApps: () => ref(installer.value),
}))

describe('AuroraApps', () => {
  it('renders a card for every category and the count line when a count is set', () => {
    installer.value = { count: '150+', categories: Object.values(InstallerAppCategory) }
    stubAuroraGlobals()

    const wrapper = mountWithKit(AuroraApps)

    expect(wrapper.find('[data-testid="apps-category-cms"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="apps-category-gallery"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="apps-count"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="aurora-apps-grid"]').attributes('data-columns')).toBe(String(balancedColumns(6)))
  })

  it('hides the count line when no count is set', () => {
    installer.value = { count: '', categories: [InstallerAppCategory.Cms] }
    stubAuroraGlobals()

    const wrapper = mountWithKit(AuroraApps)

    expect(wrapper.find('[data-testid="aurora-apps"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="apps-count"]').exists()).toBe(false)
  })

  it('renders nothing without categories', () => {
    installer.value = { count: '', categories: [] }
    stubAuroraGlobals()

    expect(mountWithKit(AuroraApps).find('[data-testid="aurora-apps"]').exists()).toBe(false)
  })
})

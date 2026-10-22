/**
 * Unit tests for SectionsApps: the fixed categories always show, the count line only when the
 * operator set a count.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import SectionsApps from '~/components/sections/SectionsApps.vue'
import { InstallerAppCategory } from '~/constants/installerAppCategory'
import { CLASSIC_COMPONENTS } from '~/tests/helpers/classicKit'

/** The installer data the stubbed composable returns. */
const installer = ref({ count: '', categories: [] as InstallerAppCategory[] })

vi.stubGlobal('useInstallerApps', () => installer)

describe('SectionsApps', () => {
  beforeEach(() => {
    installer.value = { count: '150+', categories: Object.values(InstallerAppCategory) }
  })

  it('shows one card per category', () => {
    const wrapper = mountWithKit(SectionsApps, { components: CLASSIC_COMPONENTS })

    for (const category of Object.values(InstallerAppCategory)) {
      expect(wrapper.find(`[data-testid="app-${category}"]`).exists()).toBe(true)
    }
  })

  it('shows the count line when a count is set', () => {
    const wrapper = mountWithKit(SectionsApps, { components: CLASSIC_COMPONENTS })

    expect(wrapper.find('[data-testid="sections-apps-count"]').exists()).toBe(true)
  })

  it('hides the count line when the count is empty but keeps the section', () => {
    installer.value = { count: '', categories: Object.values(InstallerAppCategory) }

    const wrapper = mountWithKit(SectionsApps, { components: CLASSIC_COMPONENTS })

    expect(wrapper.find('[data-testid="sections-apps-count"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="sections-apps"]').exists()).toBe(true)
  })

  it('draws every icon on a flat brand tile with no accent glow', () => {
    const wrapper = mountWithKit(SectionsApps, { components: CLASSIC_COMPONENTS })

    const tiles = wrapper.findAll('[data-testid="classic-icon-tile"]')
    expect(tiles.length).toBeGreaterThan(0)
    expect(wrapper.html()).not.toContain('--accent')
  })
})

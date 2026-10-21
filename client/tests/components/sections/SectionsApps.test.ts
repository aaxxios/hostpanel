/**
 * Unit tests for SectionsApps: the fixed categories always show, the count line only when the
 * operator set a count.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import SectionsApps from '~/components/sections/SectionsApps.vue'
import { InstallerAppCategory } from '~/constants/installerAppCategory'
import { ACCENT_BRAND_CLASS } from '~/utils/ui/accent'

/** The installer data the stubbed composable returns. */
const installer = ref({ count: '', categories: [] as InstallerAppCategory[] })

vi.stubGlobal('useInstallerApps', () => installer)

describe('SectionsApps', () => {
  beforeEach(() => {
    installer.value = { count: '150+', categories: Object.values(InstallerAppCategory) }
  })

  it('shows one card per category', () => {
    const wrapper = mountWithKit(SectionsApps)

    for (const category of Object.values(InstallerAppCategory)) {
      expect(wrapper.find(`[data-testid="app-${category}"]`).exists()).toBe(true)
    }
  })

  it('shows the count line when a count is set', () => {
    const wrapper = mountWithKit(SectionsApps)

    expect(wrapper.find('[data-testid="sections-apps-count"]').exists()).toBe(true)
  })

  it('hides the count line when the count is empty but keeps the section', () => {
    installer.value = { count: '', categories: Object.values(InstallerAppCategory) }

    const wrapper = mountWithKit(SectionsApps)

    expect(wrapper.find('[data-testid="sections-apps-count"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="sections-apps"]').exists()).toBe(true)
  })

  it('points every tile at the brand colour so it is visible on a light page', () => {
    const wrapper = mountWithKit(SectionsApps)

    const tiles = wrapper.findAll('[class*="--accent:"]')
    expect(tiles.length).toBeGreaterThan(0)
    for (const tile of tiles) {
      expect(tile.classes()).toContain(ACCENT_BRAND_CLASS)
    }
  })
})

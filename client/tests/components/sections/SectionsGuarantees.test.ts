/**
 * Unit tests for SectionsGuarantees: one card per guarantee that is set and no section when none is.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import SectionsGuarantees from '~/components/sections/SectionsGuarantees.vue'
import { GuaranteeKey } from '~/constants/guaranteeKey'
import type { GuaranteeItem } from '~/types/guaranteeitem'
import { ACCENT_BRAND_CLASS } from '~/utils/ui/accent'

/** The guarantees the stubbed composable returns. */
const guarantees = ref<GuaranteeItem[]>([])

vi.stubGlobal('useGuarantees', () => guarantees)

describe('SectionsGuarantees', () => {
  beforeEach(() => {
    guarantees.value = [{ key: GuaranteeKey.FreeMigration }, { key: GuaranteeKey.MoneyBack, days: 30 }]
  })

  it('shows a card for each guarantee that is set', () => {
    const wrapper = mountWithKit(SectionsGuarantees)

    expect(wrapper.find('[data-testid="guarantee-free_migration"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="guarantee-money_back"]').text()).toContain('hostingSections.guarantees.items.money_back.title')
  })

  it('leaves out a guarantee that is not set', () => {
    const wrapper = mountWithKit(SectionsGuarantees)

    expect(wrapper.find('[data-testid="guarantee-no_hidden_fees"]').exists()).toBe(false)
  })

  it('renders nothing when no guarantee is set', () => {
    guarantees.value = []

    const wrapper = mountWithKit(SectionsGuarantees)

    expect(wrapper.find('[data-testid="sections-guarantees"]').exists()).toBe(false)
  })

  it('points every tile at the brand colour so it is visible on a light page', () => {
    const wrapper = mountWithKit(SectionsGuarantees)

    const tiles = wrapper.findAll('[class*="--accent:"]')
    expect(tiles.length).toBeGreaterThan(0)
    for (const tile of tiles) {
      expect(tile.classes()).toContain(ACCENT_BRAND_CLASS)
    }
  })
})

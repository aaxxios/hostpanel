/**
 * Unit tests for SectionsGuarantees: one card per guarantee that is set and no section when none is.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import SectionsGuarantees from '~/components/sections/SectionsGuarantees.vue'
import { GuaranteeKey } from '~/constants/guaranteeKey'
import type { GuaranteeItem } from '~/types/guaranteeitem'
import { CLASSIC_COMPONENTS } from '~/tests/helpers/classicKit'

/** The guarantees the stubbed composable returns. */
const guarantees = ref<GuaranteeItem[]>([])

vi.stubGlobal('useGuarantees', () => guarantees)

describe('SectionsGuarantees', () => {
  beforeEach(() => {
    guarantees.value = [{ key: GuaranteeKey.FreeMigration }, { key: GuaranteeKey.MoneyBack, days: 30 }]
  })

  it('shows a card for each guarantee that is set', () => {
    const wrapper = mountWithKit(SectionsGuarantees, { components: CLASSIC_COMPONENTS })

    expect(wrapper.find('[data-testid="guarantee-free_migration"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="guarantee-money_back"]').text()).toContain('hostingSections.guarantees.items.money_back.title')
  })

  it('leaves out a guarantee that is not set', () => {
    const wrapper = mountWithKit(SectionsGuarantees, { components: CLASSIC_COMPONENTS })

    expect(wrapper.find('[data-testid="guarantee-no_hidden_fees"]').exists()).toBe(false)
  })

  it('renders nothing when no guarantee is set', () => {
    guarantees.value = []

    const wrapper = mountWithKit(SectionsGuarantees, { components: CLASSIC_COMPONENTS })

    expect(wrapper.find('[data-testid="sections-guarantees"]').exists()).toBe(false)
  })

  it('draws every icon on a flat brand tile with no accent glow', () => {
    const wrapper = mountWithKit(SectionsGuarantees, { components: CLASSIC_COMPONENTS })

    const tiles = wrapper.findAll('[data-testid="classic-icon-tile"]')
    expect(tiles.length).toBeGreaterThan(0)
    expect(wrapper.html()).not.toContain('--accent')
  })
})

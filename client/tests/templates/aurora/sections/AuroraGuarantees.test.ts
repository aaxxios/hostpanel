/**
 * Unit tests for AuroraGuarantees: a card per promise the composable reports and nothing without any.
 */
import { describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import AuroraGuarantees from '~/templates/aurora/sections/AuroraGuarantees.vue'
import { GuaranteeKey } from '~/constants/guaranteeKey'
import type { GuaranteeItem } from '~/types/guaranteeitem'
import { stubAuroraGlobals } from '../auroraGlobals'
import { balancedColumns } from '~/utils/balancedGrid'

/** The promises the mocked composable reports. */
const items = vi.hoisted(() => ({ value: [] as GuaranteeItem[] }))

vi.mock('~/composables/useGuarantees', () => ({
  /**
   * Stands in for the guarantees composable.
   *
   * @returns The mocked promises as a ref.
   */
  useGuarantees: () => ref(items.value),
}))

describe('AuroraGuarantees', () => {
  it('renders a card for each promise that is set', () => {
    items.value = [{ key: GuaranteeKey.FreeMigration }, { key: GuaranteeKey.MoneyBack, days: 30 }]
    stubAuroraGlobals()

    const wrapper = mountWithKit(AuroraGuarantees)

    expect(wrapper.find('[data-testid="guarantee-free_migration"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="guarantee-money_back"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="guarantee-no_hidden_fees"]').exists()).toBe(false)
  })

  it('renders nothing when no promise is set', () => {
    items.value = []
    stubAuroraGlobals()

    expect(mountWithKit(AuroraGuarantees).find('[data-testid="aurora-guarantees"]').exists()).toBe(false)
  })

  it('lays the cards out in the balanced grid', () => {
    items.value = [{ key: GuaranteeKey.FreeMigration }, { key: GuaranteeKey.MoneyBack, days: 14 }, { key: GuaranteeKey.NoHiddenFees }]
    stubAuroraGlobals()

    const grid = mountWithKit(AuroraGuarantees).find('[data-testid="aurora-guarantees-grid"]')

    expect(grid.attributes('data-columns')).toBe(String(balancedColumns(3)))
  })
})

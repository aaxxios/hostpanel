/**
 * Unit tests for NovaGuarantees: a card per guarantee the operator has set, hidden when none.
 *
 * @module tests/templates/nova/sections/NovaGuarantees.test
 */
import { computed } from 'vue'
import { describe, expect, it, vi } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import NovaGuarantees from '~/templates/nova/sections/NovaGuarantees.vue'
import { GuaranteeKey } from '~/constants/guaranteeKey'
import type { GuaranteeItem } from '~/types/guaranteeitem'
import { stubTemplateGlobals } from '~/tests/helpers/templateGlobals'

/**
 * Stands in for `useGuarantees`.
 *
 * @param items The guarantees that are set.
 */
const stubGuarantees = (items: GuaranteeItem[]): void => {
  vi.stubGlobal('useGuarantees', () => computed(() => items))
}

describe('NovaGuarantees', () => {
  it('renders one card per guarantee', () => {
    stubTemplateGlobals()
    stubGuarantees([{ key: GuaranteeKey.FreeMigration }, { key: GuaranteeKey.MoneyBack, days: 30 }])
    const wrapper = mountWithKit(NovaGuarantees)

    expect(wrapper.findAll('[data-testid="nova-guarantees-grid"] > li')).toHaveLength(2)
    expect(wrapper.find('[data-testid="nova-guarantee-money_back"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="nova-guarantee-no_hidden_fees"]').exists()).toBe(false)
  })

  it('renders nothing when no guarantee is set', () => {
    stubTemplateGlobals()
    stubGuarantees([])

    expect(mountWithKit(NovaGuarantees).find('[data-testid="nova-guarantees"]').exists()).toBe(false)
  })
})

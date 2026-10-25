/**
 * Unit tests for PayMethodSelector: the saved cards, the new-card option and the announcement
 * shown when there is no saved card.
 */
import { describe, expect, it } from 'vitest'
import { mountClient } from '~/tests/helpers/mountClient'
import PayMethodSelector from '~/components/client/invoices/PayMethodSelector.vue'
import { NEW_CARD } from '~/constants/payMethod'
import type { PaymentMethod } from '~/types/payment'

/** Two saved cards. */
const cards = [
  { id: 'pm_1', type: 'card', description: 'Visa', gateway_name: 'stripe', card_type: 'visa', card_last_four: '4242', card_expiry: '12/30' },
  { id: 'pm_2', type: 'card', description: 'Mastercard', gateway_name: 'stripe', card_type: 'mastercard', card_last_four: '1111', card_expiry: '01/31' },
] as PaymentMethod[]

describe('PayMethodSelector', () => {
  it('lists the saved cards with their last digits and expiry', () => {
    const wrapper = mountClient(PayMethodSelector, { props: { savedMethods: cards, canUseNewCard: false, modelValue: 'pm_1' } })

    expect(wrapper.text()).toContain('Visa •••• 4242')
    expect(wrapper.text()).toContain('12/30')
    expect(wrapper.find('[data-testid="pay-method-pm_2"]').exists()).toBe(true)
  })

  it('marks the chosen card', () => {
    const wrapper = mountClient(PayMethodSelector, { props: { savedMethods: cards, canUseNewCard: false, modelValue: 'pm_2' } })

    expect(wrapper.find('[data-testid="pay-method-pm_2"]').attributes('data-state')).toBe('checked')
    expect(wrapper.find('[data-testid="pay-method-pm_1"]').attributes('data-state')).toBe('unchecked')
  })

  it('emits the card that was clicked', async () => {
    const wrapper = mountClient(PayMethodSelector, { props: { savedMethods: cards, canUseNewCard: false, modelValue: 'pm_1' } })

    await wrapper.find('[data-testid="pay-method-pm_2-input"]').setValue(true)

    expect(wrapper.emitted('update:modelValue')).toEqual([['pm_2']])
  })

  it('offers the new-card option only when the hand-off exists', async () => {
    const without = mountClient(PayMethodSelector, { props: { savedMethods: cards, canUseNewCard: false, modelValue: 'pm_1' } })
    const withNew = mountClient(PayMethodSelector, { props: { savedMethods: cards, canUseNewCard: true, modelValue: 'pm_1' } })

    expect(without.find('[data-testid="pay-method-new"]').exists()).toBe(false)

    await withNew.find('[data-testid="pay-method-new-input"]').setValue(true)
    expect(withNew.emitted('update:modelValue')).toEqual([[NEW_CARD]])
  })

  it('announces the card form when there is no saved card', () => {
    const wrapper = mountClient(PayMethodSelector, { props: { savedMethods: [], canUseNewCard: true, modelValue: NEW_CARD } })

    expect(wrapper.find('[data-testid="pay-method-enter-card"]').exists()).toBe(true)
  })

  it('shows nothing under the title when no card can be used at all', () => {
    const wrapper = mountClient(PayMethodSelector, { props: { savedMethods: [], canUseNewCard: false, modelValue: null } })

    expect(wrapper.find('[data-testid="pay-method-enter-card"]').exists()).toBe(false)
    expect(wrapper.findAll('[role="radio"]')).toHaveLength(0)
  })
})

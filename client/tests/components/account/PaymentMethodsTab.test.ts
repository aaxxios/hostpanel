/**
 * Unit tests for PaymentMethodsTab: loading, failed read vs empty, actions and the add hand-off.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { mountAccount } from '~/tests/helpers/mountAccount'
import PaymentMethodsTab from '~/components/account/PaymentMethodsTab.vue'
import { useAccountStore } from '~/stores/account'
import { ActionOutcome } from '~/types/actionoutcome'
import type { PaymentMethod } from '~/types/payment'

vi.mock('~/composables/apis/useClientApi', () => ({ useClientApi: () => ({}) }))
vi.mock('~/composables/apis/useBillingApi', () => ({ useBillingApi: () => ({}) }))
vi.mock('~/composables/apis/useAuthApi', () => ({ useAuthApi: () => ({}) }))
vi.mock('qrcode', () => ({ default: { toDataURL: async (): Promise<string> => '' } }))

/** One saved card. */
const method: PaymentMethod = { id: '9', type: 'card', description: 'My card', gateway_name: 'stripe', card_last_four: '4242' }

describe('PaymentMethodsTab', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('shows a skeleton while loading', async () => {
    const wrapper = mountAccount(PaymentMethodsTab)
    useAccountStore().paymentLoading = true
    await wrapper.vm.$nextTick()

    expect(wrapper.find('[data-testid="account-payment-loading"]').exists()).toBe(true)
  })

  it('shows the empty state when the read worked and nothing is saved', () => {
    const wrapper = mountAccount(PaymentMethodsTab)

    expect(wrapper.find('[data-testid="ui-empty-state"]').text()).toContain('client.payment.empty')
    expect(wrapper.find('[data-testid="account-payment-load-error"]').exists()).toBe(false)
  })

  it('shows the failure, not the empty state, when the read failed', async () => {
    const wrapper = mountAccount(PaymentMethodsTab)
    useAccountStore().paymentLoadError = 'Billing is down'
    await wrapper.vm.$nextTick()

    expect(wrapper.find('[data-testid="account-payment-load-error"]').text()).toContain('Billing is down')
    expect(wrapper.find('[data-testid="ui-empty-state"]').exists()).toBe(false)
  })

  it('does not offer "add" without a billing-portal URL', () => {
    const wrapper = mountAccount(PaymentMethodsTab)

    expect(wrapper.find('[data-testid="account-payment-add"]').exists()).toBe(false)
  })

  it('opens the billing portal in a new tab when a URL is configured', async () => {
    vi.stubGlobal('useRuntimeConfig', () => ({ public: { billingPortalUrl: 'https://billing.example.com' } }))
    const open = vi.spyOn(window, 'open').mockReturnValue(null)
    const wrapper = mountAccount(PaymentMethodsTab)

    await wrapper.find('[data-testid="account-payment-add"]').trigger('click')

    expect(open).toHaveBeenCalledWith('https://billing.example.com/clientarea.php?action=creditcard', '_blank')
  })

  it('makes a method the default and reports it', async () => {
    const wrapper = mountAccount(PaymentMethodsTab)
    const store = useAccountStore()
    store.paymentMethods = [method]
    vi.spyOn(store, 'setDefaultPaymentMethod').mockResolvedValue(ActionOutcome.Succeeded)
    const reload = vi.spyOn(store, 'loadPaymentMethods').mockResolvedValue()
    await wrapper.vm.$nextTick()

    await wrapper.find('[data-testid="account-payment-default-9"]').trigger('click')
    await Promise.resolve()
    await wrapper.vm.$nextTick()

    expect(reload).toHaveBeenCalled()
    expect(wrapper.find('[data-testid="account-payment-success"]').text()).toBe('client.payment.defaultSet')
  })

  it('removes a method only after confirmation', async () => {
    const wrapper = mountAccount(PaymentMethodsTab)
    const store = useAccountStore()
    store.paymentMethods = [method]
    const remove = vi.spyOn(store, 'removePaymentMethod').mockResolvedValue(ActionOutcome.Succeeded)
    vi.spyOn(store, 'loadPaymentMethods').mockResolvedValue()
    await wrapper.vm.$nextTick()

    await wrapper.find('[data-testid="account-payment-remove-9"]').trigger('click')
    expect(remove).not.toHaveBeenCalled()

    await wrapper.find('[data-testid="ui-confirm-confirm"]').trigger('click')
    await Promise.resolve()

    expect(remove).toHaveBeenCalledWith('9')
  })

  it('shows the API sentence of a refused action', async () => {
    const wrapper = mountAccount(PaymentMethodsTab)
    useAccountStore().paymentActionError = 'Cannot remove the last method'
    await wrapper.vm.$nextTick()

    expect(wrapper.find('[data-testid="account-payment-action-error"]').text()).toContain('Cannot remove the last method')
  })
})

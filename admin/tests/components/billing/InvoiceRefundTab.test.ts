/**
 * Mount tests for InvoiceRefundTab: the notices that replace the form, the credit question that
 * gates the refund button, and sending a refund through the store.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import InvoiceRefundTab from '@/components/billing/InvoiceRefundTab.vue'
import UiSelect from '@/components/ui/UiSelect.vue'
import { invoiceFixture } from '../../helpers/billingFixtures'
import { json } from '../../helpers/fetch'
import { mountWithApp } from '../../helpers/mount'
import { stubRoutes } from '../../helpers/routes'

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('InvoiceRefundTab', () => {
  it('replaces the form with a notice for a draft, a cancelled and a refunded invoice', () => {
    const draft = mountWithApp(InvoiceRefundTab, { props: { invoice: invoiceFixture({ status: 'Draft' }) } })
    const cancelled = mountWithApp(InvoiceRefundTab, { props: { invoice: invoiceFixture({ status: 'Cancelled' }) } })
    const refunded = mountWithApp(InvoiceRefundTab, { props: { invoice: invoiceFixture({ status: 'Refunded' }) } })

    expect(draft.find('[data-testid="billing-invoice-refund-draft"]').exists()).toBe(true)
    expect(cancelled.find('[data-testid="billing-invoice-refund-cancelled"]').exists()).toBe(true)
    expect(refunded.find('[data-testid="billing-invoice-refund-refunded"]').exists()).toBe(true)
    expect(draft.find('[data-testid="billing-invoice-refund-form"]').exists()).toBe(false)
  })

  it('warns read-only when no payment can be refunded', () => {
    const wrapper = mountWithApp(InvoiceRefundTab, {
      props: { invoice: invoiceFixture({ transactions: [] }), readonly: true },
    })

    expect(wrapper.find('[data-testid="billing-invoice-refund-none"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="billing-invoice-refund-form"]').exists()).toBe(false)
  })

  it('asks what to do with the credit and enables refund only once answered', async () => {
    const wrapper = mountWithApp(InvoiceRefundTab, { props: { invoice: invoiceFixture() } })
    expect(wrapper.find('[data-testid="billing-invoice-refund-credit"]').exists()).toBe(false)

    wrapper.findAllComponents(UiSelect)[0]?.vm.$emit('update:modelValue', '21')
    await flushPromises()
    expect(wrapper.find('[data-testid="billing-invoice-refund-credit"]').exists()).toBe(true)
    expect(wrapper.get('[data-testid="billing-invoice-refund-submit"]').attributes('disabled')).toBeDefined()

    await wrapper.get('[data-testid="billing-invoice-refund-credit-leave"]').setValue(true)
    expect(wrapper.get('[data-testid="billing-invoice-refund-submit"]').attributes('disabled')).toBeUndefined()
  })

  it('sends the refund with the payment amount and tells the parent', async () => {
    const mock = stubRoutes({
      'POST /api/billing/1/refund': () => new Response(null, { status: 204 }),
      'GET /api/billing/1': () => json(invoiceFixture()),
    })
    const wrapper = mountWithApp(InvoiceRefundTab, { props: { invoice: invoiceFixture() } })

    wrapper.findAllComponents(UiSelect)[0]?.vm.$emit('update:modelValue', '21')
    await flushPromises()
    await wrapper.get('[data-testid="billing-invoice-refund-credit-leave"]').setValue(true)
    await wrapper.get('[data-testid="billing-invoice-refund-form"]').trigger('submit')
    await flushPromises()

    const post = mock.mock.calls.find(([url]) => String(url).endsWith('/billing/1/refund'))
    expect(JSON.parse(String((post?.[1] as RequestInit | undefined)?.body))).toMatchObject({
      transactionId: '21',
      amount: 40,
      refundType: 'Gateway',
      gateway: 'Stripe',
    })
    expect(wrapper.emitted('updated')).toHaveLength(1)
  })
})

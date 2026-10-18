/**
 * Mount tests for InvoiceSummaryTab: the invoice facts, the action buttons that depend on the
 * status, the read-only mode, and the mark-paid action going through the store.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import InvoiceSummaryTab from '@/components/billing/InvoiceSummaryTab.vue'
import { invoiceFixture } from '../../helpers/billingFixtures'
import { json, problem } from '../../helpers/fetch'
import { mountWithApp } from '../../helpers/mount'
import { stubRoutes } from '../../helpers/routes'

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('InvoiceSummaryTab', () => {
  it('shows the client, the status and the balance left after the payments', () => {
    const wrapper = mountWithApp(InvoiceSummaryTab, { props: { invoice: invoiceFixture() } })

    expect(wrapper.get('[data-testid="billing-invoice-client-name"]').text()).toBe('Ada Lovelace')
    expect(wrapper.get('[data-testid="billing-invoice-status"]').text()).toBe('Unpaid')
    expect(wrapper.get('[data-testid="billing-invoice-fact-balance"]').text()).toContain('60.00 USD')
  })

  it('shows no balance for a paid invoice settled without a payment row of its own', () => {
    const wrapper = mountWithApp(InvoiceSummaryTab, { props: { invoice: invoiceFixture({ status: 'Paid', transactions: [] }) } })

    expect(wrapper.get('[data-testid="billing-invoice-fact-balance"]').text()).toContain('0.00 USD')
    expect(wrapper.get('[data-testid="billing-invoice-fact-balance"]').text()).not.toContain('60.00')
  })

  it('offers publish for a draft and capture or mark paid for an unpaid invoice', () => {
    const draft = mountWithApp(InvoiceSummaryTab, { props: { invoice: invoiceFixture({ status: 'Draft' }) } })
    expect(draft.find('[data-testid="billing-invoice-publish"]').exists()).toBe(true)
    expect(draft.find('[data-testid="billing-invoice-mark-paid"]').exists()).toBe(false)
    expect(draft.find('[data-testid="billing-invoice-draft-notice"]').exists()).toBe(true)

    const unpaid = mountWithApp(InvoiceSummaryTab, { props: { invoice: invoiceFixture() } })
    expect(unpaid.find('[data-testid="billing-invoice-capture"]').exists()).toBe(true)
    expect(unpaid.find('[data-testid="billing-invoice-mark-paid"]').exists()).toBe(true)
  })

  it('hides every action when read-only', () => {
    const wrapper = mountWithApp(InvoiceSummaryTab, { props: { invoice: invoiceFixture(), readonly: true } })

    expect(wrapper.find('[data-testid="billing-invoice-mark-paid"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="billing-invoice-cancel"]').exists()).toBe(false)
  })

  it('marks the invoice paid through the bulk endpoint and tells the parent', async () => {
    const mock = stubRoutes({
      'POST /api/billing/bulk': () => new Response(null, { status: 204 }),
      'GET /api/billing/1': () => json(invoiceFixture({ status: 'Paid' })),
    })
    const wrapper = mountWithApp(InvoiceSummaryTab, { props: { invoice: invoiceFixture() } })

    await wrapper.get('[data-testid="billing-invoice-mark-paid"]').trigger('click')
    await flushPromises()

    const post = mock.mock.calls.find(([url]) => String(url).endsWith('/billing/bulk'))
    expect(JSON.parse(String((post?.[1] as RequestInit | undefined)?.body))).toEqual({ invoiceIds: [1], action: 'MarkPaid' })
    expect(wrapper.emitted('updated')).toHaveLength(1)
  })

  it('does not tell the parent when the action is refused', async () => {
    stubRoutes({ 'POST /api/billing/1/cancel': () => problem('Cannot cancel.', 409) })
    const wrapper = mountWithApp(InvoiceSummaryTab, { props: { invoice: invoiceFixture() } })

    await wrapper.get('[data-testid="billing-invoice-cancel"]').trigger('click')
    await flushPromises()

    expect(wrapper.emitted('updated')).toBeUndefined()
  })
})

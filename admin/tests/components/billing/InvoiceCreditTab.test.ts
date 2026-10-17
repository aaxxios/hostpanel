/**
 * Mount tests for InvoiceCreditTab: the guards on the two credit buttons and applying credit
 * through the store.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import InvoiceCreditTab from '@/components/billing/InvoiceCreditTab.vue'
import { invoiceFixture } from '../../helpers/billingFixtures'
import { json } from '../../helpers/fetch'
import { mountWithApp } from '../../helpers/mount'
import { stubRoutes } from '../../helpers/routes'

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('InvoiceCreditTab', () => {
  it('keeps both buttons disabled until an amount is typed', () => {
    const wrapper = mountWithApp(InvoiceCreditTab, { props: { invoice: invoiceFixture() } })

    expect(wrapper.get('[data-testid="billing-invoice-credit-add"]').attributes('disabled')).toBeDefined()
    expect(wrapper.get('[data-testid="billing-invoice-credit-remove"]').attributes('disabled')).toBeDefined()
  })

  it('shows the credit available under each box', () => {
    const wrapper = mountWithApp(InvoiceCreditTab, { props: { invoice: invoiceFixture() } })

    expect(wrapper.text()).toContain('5.00 USD Available')
  })

  it('applies the credit and tells the parent', async () => {
    const mock = stubRoutes({
      'POST /api/billing/1/credit': () => new Response(null, { status: 204 }),
      'GET /api/billing/1': () => json(invoiceFixture()),
    })
    const wrapper = mountWithApp(InvoiceCreditTab, { props: { invoice: invoiceFixture() } })

    await wrapper.get('[data-testid="billing-invoice-credit-add-amount"] input').setValue('20')
    await wrapper.get('[data-testid="billing-invoice-credit-add"]').trigger('click')
    await flushPromises()

    const post = mock.mock.calls.find(([url]) => String(url).endsWith('/billing/1/credit'))
    expect(JSON.parse(String((post?.[1] as RequestInit | undefined)?.body))).toEqual({ amount: 20 })
    expect(wrapper.emitted('updated')).toHaveLength(1)
  })

  it('takes credit back through the remove endpoint', async () => {
    const mock = stubRoutes({
      'POST /api/billing/1/credit/remove': () => new Response(null, { status: 204 }),
      'GET /api/billing/1': () => json(invoiceFixture()),
    })
    const wrapper = mountWithApp(InvoiceCreditTab, { props: { invoice: invoiceFixture() } })

    await wrapper.get('[data-testid="billing-invoice-credit-remove-amount"] input').setValue('3')
    await wrapper.get('[data-testid="billing-invoice-credit-remove"]').trigger('click')
    await flushPromises()

    expect(mock.mock.calls.some(([url]) => String(url).endsWith('/billing/1/credit/remove'))).toBe(true)
  })
})

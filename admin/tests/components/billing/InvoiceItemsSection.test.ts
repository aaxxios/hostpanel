/**
 * Mount tests for InvoiceItemsSection: the read-only table with its totals, the editor (add,
 * tick and delete lines) and the save that goes through the billing store.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import InvoiceItemsSection from '@/components/billing/InvoiceItemsSection.vue'
import { invoiceFixture } from '../../helpers/billingFixtures'
import { json } from '../../helpers/fetch'
import { mountWithApp } from '../../helpers/mount'
import { stubRoutes } from '../../helpers/routes'

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('InvoiceItemsSection', () => {
  it('shows the lines and the totals read-only', () => {
    const wrapper = mountWithApp(InvoiceItemsSection, { props: { invoice: invoiceFixture(), readonly: true } })

    expect(wrapper.get('[data-testid="billing-invoice-items-table"]').text()).toContain('Managed hosting')
    expect(wrapper.get('[data-testid="billing-invoice-total-total"]').text()).toBe('100.00 USD')
    expect(wrapper.find('[data-testid="billing-invoice-items-save"]').exists()).toBe(false)
  })

  it('adds a line, removes it again, and deletes a ticked line', async () => {
    const wrapper = mountWithApp(InvoiceItemsSection, { props: { invoice: invoiceFixture() } })

    await wrapper.get('[data-testid="billing-invoice-items-add"]').trigger('click')
    expect(wrapper.find('[data-testid="billing-invoice-item-description-1"]').exists()).toBe(true)

    await wrapper.get('[data-testid="billing-invoice-item-remove-1"]').trigger('click')
    expect(wrapper.find('[data-testid="billing-invoice-item-description-1"]').exists()).toBe(false)

    await wrapper.get('[data-testid="billing-invoice-item-select-0"]').trigger('click')
    await wrapper.get('[data-testid="billing-invoice-items-delete-selected"]').trigger('click')
    expect(wrapper.find('[data-testid="billing-invoice-item-description-0"]').exists()).toBe(false)
  })

  it('saves the lines through the store and tells the parent', async () => {
    const mock = stubRoutes({
      'PUT /api/billing/1/items': () => new Response(null, { status: 204 }),
      'GET /api/billing/1': () => json(invoiceFixture()),
    })
    const wrapper = mountWithApp(InvoiceItemsSection, { props: { invoice: invoiceFixture() } })

    await wrapper.get('[data-testid="billing-invoice-items-save"]').trigger('click')
    await flushPromises()

    const put = mock.mock.calls.find(([, init]) => (init as RequestInit | undefined)?.method === 'PUT')
    expect(JSON.parse(String((put?.[1] as RequestInit | undefined)?.body))).toEqual({
      items: [{ id: 11, description: 'Managed hosting', unitPrice: 100, quantity: 1, isDeleted: false }],
    })
    expect(wrapper.emitted('updated')).toHaveLength(1)
  })

  it('previews the tax on taxed lines when a tax percentage is given', async () => {
    const wrapper = mountWithApp(InvoiceItemsSection, {
      props: { invoice: invoiceFixture({ taxRate: 10 }), taxPercent: 10 },
    })
    await flushPromises()

    expect(wrapper.get('[data-testid="billing-invoice-total-tax"]').text()).toBe('10.00 USD')
    expect(wrapper.get('[data-testid="billing-invoice-total-total"]').text()).toBe('105.00 USD')
  })
})

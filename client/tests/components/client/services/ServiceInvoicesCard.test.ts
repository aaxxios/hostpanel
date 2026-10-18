/**
 * Unit tests for ServiceInvoicesCard: loading, the failure that is never shown as "no invoices",
 * the empty state, the table and the note about invoices no service owns.
 */
import { describe, expect, it, vi } from 'vitest'
import { mountClient } from '~/tests/helpers/mountClient'
import ServiceInvoicesCard from '~/components/client/services/ServiceInvoicesCard.vue'
import { useServiceManagementStore } from '~/stores/servicemanagement'
import type { ServiceInvoice } from '~/types/serviceinvoice'

vi.mock('~/composables/apis/useClientApi', () => ({ useClientApi: () => ({}) }))

/**
 * Builds an invoice row.
 *
 * @param id - Invoice id.
 * @returns The invoice.
 */
const invoice = (id: number): ServiceInvoice => ({ id, invoiceDate: '2026-01-01', dueDate: '2026-01-15', total: 10, currency: 'USD', status: 'Paid' })

/**
 * Mounts the card and sets the store state it renders.
 *
 * @param state - The store fields to set.
 * @returns The wrapper.
 */
const mountCard = async (state: Record<string, unknown>) => {
  const wrapper = mountClient(ServiceInvoicesCard)
  Object.assign(useServiceManagementStore(), { invoicesLoading: false, ...state })
  await wrapper.vm.$nextTick()
  return wrapper
}

describe('ServiceInvoicesCard', () => {
  it('shows skeletons while loading', async () => {
    const wrapper = await mountCard({ invoicesLoading: true })

    expect(wrapper.findAll('[data-testid="ui-skeleton"]')).toHaveLength(3)
  })

  it('shows the failure, not an empty list, when the read failed', async () => {
    const wrapper = await mountCard({ invoicesError: 'Could not read invoices.' })

    expect(wrapper.find('[data-testid="service-invoices-error"]').text()).toBe('Could not read invoices.')
    expect(wrapper.find('[data-testid="service-invoices-empty"]').exists()).toBe(false)
  })

  it('says nothing is recorded against the service when the list is empty', async () => {
    const wrapper = await mountCard({ relatedInvoices: [] })

    expect(wrapper.find('[data-testid="service-invoices-empty"]').exists()).toBe(true)
  })

  it('lists the invoices with a link to each', async () => {
    const wrapper = await mountCard({ relatedInvoices: [invoice(5), invoice(6)] })

    expect(wrapper.findAll('tbody tr')).toHaveLength(2)
    expect(wrapper.find('[data-testid="service-invoice-5"] a').attributes('href')).toBe('/client/invoices/5')
  })

  it('notes the invoices that no service owns, but not when the read failed', async () => {
    const ok = await mountCard({ relatedInvoices: [invoice(5)], unattributedInvoices: 3 })
    const failed = await mountCard({ invoicesError: 'x', unattributedInvoices: 3 })

    expect(ok.find('[data-testid="service-invoices-unrecorded"]').exists()).toBe(true)
    expect(failed.find('[data-testid="service-invoices-unrecorded"]').exists()).toBe(false)
  })
})

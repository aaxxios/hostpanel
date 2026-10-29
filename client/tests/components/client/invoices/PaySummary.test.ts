/**
 * Unit tests for PaySummary: the line items, the totals and the balance still due.
 */
import { describe, expect, it } from 'vitest'
import { mountClient } from '~/tests/helpers/mountClient'
import PaySummary from '~/components/client/invoices/PaySummary.vue'
import type { ClientInvoice } from '~/types/clientinvoice'
import { InvoiceTransactionType } from '~/types/invoicetransactiontype'

/** An invoice of 100, part-paid by 40, with 10 of that refunded. */
const invoice = {
  id: 9, currency: 'USD', subTotal: 100, credit: 0, total: 100, amountDue: 60,
  items: [{ id: 1, description: 'Hosting', amount: 60 }, { id: 2, description: 'Domain', amount: 40 }],
  transactions: [
    { id: 1, amount: 40, type: InvoiceTransactionType.Payment },
    { id: 2, amount: 10, type: InvoiceTransactionType.Refund }
  ]
} as unknown as ClientInvoice

describe('PaySummary', () => {
  it('titles the card with the invoice number', () => {
    const wrapper = mountClient(PaySummary, { props: { invoice } })

    expect(wrapper.find('h2').text()).toContain('invoicePay.invoiceNum')
  })

  it('lists every line item in the invoice currency', () => {
    const wrapper = mountClient(PaySummary, { props: { invoice } })

    expect(wrapper.text()).toContain('Hosting')
    expect(wrapper.text()).toContain('$60.00')
    expect(wrapper.text()).toContain('Domain')
  })

  it('shows what was paid and what is still due, not counting the refund as paid', () => {
    const wrapper = mountClient(PaySummary, { props: { invoice } })

    expect(wrapper.text()).toContain('$40.00')
    expect(wrapper.find('[data-testid="pay-balance-due"]').text()).toContain('$60.00')
  })
})

/**
 * Mount tests for TransactionsListView: the ledger totals and rows render, the description
 * filter narrows the page, a delete asks first, and a refused load shows the API sentence.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import TransactionsListView from '@/views/TransactionsListView.vue'
import { transactionFixture } from '../helpers/billingFixtures'
import { json, problem } from '../helpers/fetch'
import { mountRouted } from '../helpers/mount'
import { stubRoutes } from '../helpers/routes'

// The chart needs a canvas the test environment does not have; TransactionChart has its own test.
vi.mock('@/components/billing/TransactionChart.vue', () => ({ default: { name: 'TransactionChart', render: () => null } }))

/** The ledger endpoint answer: two transactions and the ledger totals. */
const ledger = () =>
  json({
    transactions: {
      items: [transactionFixture(), transactionFixture({ id: 10, description: 'Refund to client', amountIn: 0, amountOut: 20 })],
      totalCount: 2,
      page: 1,
      pageSize: 25,
    },
    totalIn: 120,
    totalOut: 20,
    totalFees: 3.5,
    balance: 96.5,
  })

/**
 * Mounts the ledger and waits for its first load.
 *
 * @returns The wrapper.
 */
const mountLedger = async () => {
  const { wrapper } = await mountRouted(TransactionsListView, '/billing/transactions', '/billing/transactions')
  await flushPromises()
  return wrapper
}

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('TransactionsListView', () => {
  it('shows the ledger totals and a row per transaction', async () => {
    stubRoutes({ 'GET /api/transactions?': ledger })
    const wrapper = await mountLedger()

    expect(wrapper.get('[data-testid="billing-transactions-total-in"]').text()).toContain('120.00 USD')
    expect(wrapper.get('[data-testid="billing-transactions-balance"]').text()).toContain('96.50 USD')
    expect(wrapper.findAll('[data-testid="billing-transactions-table"] tbody tr')).toHaveLength(2)
    expect(wrapper.get('[data-testid="billing-transaction-edit-9"]').attributes('href')).toBe('/billing/transactions/9/edit')
  })

  it('narrows the page by description', async () => {
    stubRoutes({ 'GET /api/transactions?': ledger })
    const wrapper = await mountLedger()

    await wrapper.get('[data-testid="billing-transactions-filter-toggle"]').trigger('click')
    await wrapper.get('[data-testid="billing-transactions-filter-description"]').setValue('refund')

    expect(wrapper.findAll('[data-testid="billing-transactions-table"] tbody tr')).toHaveLength(1)
  })

  it('deletes a transaction only after the confirmation', async () => {
    const mock = stubRoutes({
      'GET /api/transactions?': ledger,
      'DELETE /api/transactions/9': () => new Response(null, { status: 204 }),
    })
    const wrapper = await mountLedger()

    await wrapper.get('[data-testid="billing-transaction-delete-9"]').trigger('click')
    expect(mock.mock.calls.some(([, init]) => (init as RequestInit | undefined)?.method === 'DELETE')).toBe(false)
    await wrapper.get('[data-testid="dialog-confirm"]').trigger('click')
    await flushPromises()

    expect(mock.mock.calls.some(([, init]) => (init as RequestInit | undefined)?.method === 'DELETE')).toBe(true)
  })

  it('shows the API sentence when the load is refused', async () => {
    stubRoutes({ 'GET /api/transactions?': () => problem('Ledger is unavailable.', 403) })
    const wrapper = await mountLedger()

    expect(wrapper.get('[data-testid="billing-transactions-error"]').text()).toContain('Ledger is unavailable.')
  })
})

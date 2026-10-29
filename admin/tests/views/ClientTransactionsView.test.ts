/**
 * Mount tests for ClientTransactionsView: it renders against the real kit and settles on an API
 * refusal with a Retry, and a delete is confirmed first and reports its outcome next to the ledger.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import ClientTransactionsView from '@/views/ClientTransactionsView.vue'
import { transactionFixture } from '../helpers/billingFixtures'
import { mountClientView } from '../helpers/clientViews'
import { json, problem } from '../helpers/fetch'
import { mountRouted } from '../helpers/mount'
import { stubRoutes } from '../helpers/routes'

/** The one-transaction ledger of client 7. */
const ledger = () =>
  json({
    transactions: { items: [transactionFixture()], pageNumber: 1, pageSize: 25, totalCount: 1, totalPages: 1 },
    totalIn: 120,
    totalOut: 0,
    totalFees: 3.5,
    balance: 116.5,
    currency: 'USD',
  })

/**
 * Mounts the screen for client 7 and waits for its first load.
 *
 * @returns The wrapper.
 */
const mountLedger = async () => {
  const { wrapper } = await mountRouted(ClientTransactionsView, '/clients/:id/transactions', '/clients/7/transactions')
  await flushPromises()
  return wrapper
}

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('ClientTransactionsView', () => {
  it('mounts without throwing', async () => {
    const wrapper = await mountClientView(ClientTransactionsView)

    expect(wrapper.exists()).toBe(true)
  })

  it('renders its main action', async () => {
    const wrapper = await mountClientView(ClientTransactionsView)

    expect(wrapper.find('[data-testid="client-transactions-add"]').exists()).toBe(true)
  })

  it('shows the refusal sentence from the API', async () => {
    const wrapper = await mountClientView(ClientTransactionsView, 'Server said no')

    expect(wrapper.text()).toContain('Server said no')
  })

  it('reports a deleted transaction next to the ledger', async () => {
    stubRoutes({ 'GET /api/transactions/client/7': ledger, 'DELETE /api/transactions/9': () => new Response(null, { status: 204 }) })
    const wrapper = await mountLedger()

    await wrapper.get('[data-testid="client-transactions-delete-9"]').trigger('click')
    await wrapper.get('[data-testid="dialog-confirm"]').trigger('click')
    await flushPromises()

    expect(wrapper.get('[data-testid="client-transactions-feedback"]').text()).toBe('Transaction deleted.')
  })

  it('shows the API sentence when the delete is refused, without the load error', async () => {
    stubRoutes({ 'GET /api/transactions/client/7': ledger, 'DELETE /api/transactions/9': () => problem('The transaction is reconciled.', 409) })
    const wrapper = await mountLedger()

    await wrapper.get('[data-testid="client-transactions-delete-9"]').trigger('click')
    await wrapper.get('[data-testid="dialog-confirm"]').trigger('click')
    await flushPromises()

    expect(wrapper.get('[data-testid="client-transactions-feedback"]').text()).toBe('The transaction is reconciled.')
    expect(wrapper.find('[data-testid="client-transactions-error"]').exists()).toBe(false)
  })

  it('does not delete when the confirmation is cancelled', async () => {
    const mock = stubRoutes({ 'GET /api/transactions/client/7': ledger })
    const wrapper = await mountLedger()

    await wrapper.get('[data-testid="client-transactions-delete-9"]').trigger('click')
    await wrapper.get('[data-testid="dialog-cancel"]').trigger('click')
    await flushPromises()

    expect(mock.mock.calls.some(([, init]) => (init as RequestInit | undefined)?.method === 'DELETE')).toBe(false)
  })

  it('reloads the ledger from the Retry of a refused load', async () => {
    let refused = true
    stubRoutes({ 'GET /api/transactions/client/7': () => (refused ? problem('Ledger is unavailable.', 503) : ledger()) })
    const wrapper = await mountLedger()

    refused = false
    await wrapper.get('[data-testid="client-transactions-error-retry"]').trigger('click')
    await flushPromises()

    expect(wrapper.find('[data-testid="client-transactions-error"]').exists()).toBe(false)
  })
})

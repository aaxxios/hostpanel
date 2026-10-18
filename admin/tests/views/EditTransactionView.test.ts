/**
 * Mount tests for EditTransactionView: the stored transaction fills the form with its client
 * locked, and saving puts the edited fields and returns to the ledger.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import EditTransactionView from '@/views/EditTransactionView.vue'
import { transactionFixture } from '../helpers/billingFixtures'
import { json } from '../helpers/fetch'
import { mountRouted } from '../helpers/mount'
import { stubRoutes } from '../helpers/routes'

/** The clients endpoint answer: the client of the stored transaction. */
const clients = () =>
  json({
    items: [{ id: 7, firstName: 'Ada', lastName: 'Lovelace', email: 'ada@example.com', status: 'Active' }],
    totalCount: 1,
  })

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('EditTransactionView', () => {
  it('fills the form from the stored transaction and shows the client as text', async () => {
    stubRoutes({ 'GET /api/clients': clients, 'GET /api/transactions/9': () => json(transactionFixture()) })
    const { wrapper } = await mountRouted(EditTransactionView, '/billing/transactions/:id/edit', '/billing/transactions/9/edit')
    await flushPromises()

    expect((wrapper.get('[data-testid="billing-transaction-description"]').element as HTMLInputElement).value).toBe('Hosting payment')
    expect(wrapper.get('[data-testid="billing-transaction-client-locked"]').text()).toBe('Ada Lovelace')
  })

  it('puts the edited description and returns to the ledger', async () => {
    const mock = stubRoutes({
      'GET /api/clients': clients,
      'GET /api/transactions/9': () => json(transactionFixture()),
      'PUT /api/transactions/9': () => new Response(null, { status: 204 }),
    })
    const { wrapper, router } = await mountRouted(EditTransactionView, '/billing/transactions/:id/edit', '/billing/transactions/9/edit')
    await flushPromises()

    await wrapper.get('[data-testid="billing-transaction-description"]').setValue('Hosting payment (fixed)')
    await wrapper.get('form').trigger('submit')
    await flushPromises()

    const put = mock.mock.calls.find(([, init]) => (init as RequestInit | undefined)?.method === 'PUT')
    expect(JSON.parse(String((put?.[1] as RequestInit | undefined)?.body))).toMatchObject({
      description: 'Hosting payment (fixed)',
      invoiceId: 1,
      amountIn: 120,
    })
    expect(router.currentRoute.value.path).toBe('/billing/transactions')
  })
})

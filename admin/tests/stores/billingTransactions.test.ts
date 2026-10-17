/**
 * Tests for the billing transactions store and the billable items store: loads fill the
 * state, writes reload the list, and refusals land in the store instead of a view.
 */

import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { useBillableItemsStore } from '@/stores/billableItems'
import { useTransactionsStore } from '@/stores/billingTransactions'
import { json, problem, stubFetch, stubOffline, callOf } from '../helpers/fetch'

/** Sentence the client writes when nothing answered. */
const NO_ANSWER = 'Could not reach the server. Check your connection and try again.'

/** The ledger answer the list endpoint sends. */
const ledger = {
  transactions: { items: [{ id: 1 }], totalCount: 4, page: 1, pageSize: 25 },
  totalIn: 10,
  totalOut: 2,
  totalFees: 1,
  balance: 7,
}

/** Fields a transaction body carries. */
const fields = {
  date: 'd',
  description: 'x',
  transactionId: 't',
  invoiceId: null,
  paymentMethod: 'Manual',
  amountIn: 1,
  amountOut: 0,
  fees: 0,
}

beforeEach(() => {
  setActivePinia(createPinia())
})

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('billingTransactionsStore', () => {
  it('fetchAll fills the page and the summary and remembers the paging', async () => {
    const mock = stubFetch(json(ledger))
    const store = useTransactionsStore()

    await store.fetchAll(2, 10)
    expect(store.transactions).toEqual([{ id: 1 }])
    expect(store.totalCount).toBe(4)
    expect(store.summary).toEqual({ totalIn: 10, totalOut: 2, totalFees: 1, balance: 7 })
    expect(store.page).toBe(2)
    expect(callOf(mock).url).toBe('/api/transactions?page=2&pageSize=10')
  })

  it('fetchAll shows the API sentence on refusal and the no-answer sentence offline', async () => {
    const store = useTransactionsStore()
    stubFetch(problem('Ledger is locked.', 403))
    await store.fetchAll()
    expect(store.error).toBe('Ledger is locked.')

    stubOffline()
    await store.fetchAll()
    expect(store.error).toBe(NO_ANSWER)
  })

  it('fetchById sets the open transaction', async () => {
    stubFetch(json({ id: 2 }))
    const store = useTransactionsStore()

    await store.fetchById(2)
    expect(store.currentTransaction).toEqual({ id: 2 })
  })

  it('create reloads the current page, and a refusal returns false with field errors', async () => {
    const store = useTransactionsStore()
    const mock = stubFetch(new Response(null, { status: 204 }), json(ledger))
    expect(await store.create({ ...fields, clientId: 1, addToCredit: false })).toBe(true)
    expect(mock).toHaveBeenCalledTimes(2)

    stubFetch(json({ type: 'VALIDATION_ERROR', title: 'Invalid', status: 400, detail: 'Fix the form.', errors: { AmountIn: ['Amount is required.'] } }, 400))
    expect(await store.create({ ...fields, clientId: 1, addToCredit: false })).toBe(false)
    expect(store.fieldErrors).toEqual({ amountIn: 'Amount is required.' })
  })

  it('update returns the outcome without reloading', async () => {
    const store = useTransactionsStore()
    const mock = stubFetch(new Response(null, { status: 204 }))
    expect(await store.update(2, fields)).toBe(true)
    expect(mock).toHaveBeenCalledTimes(1)

    stubFetch(problem('Transaction is settled.', 409))
    expect(await store.update(2, fields)).toBe(false)
    expect(store.error).toBe('Transaction is settled.')
  })

  it('remove reloads the current page on success only', async () => {
    const store = useTransactionsStore()
    const mock = stubFetch(new Response(null, { status: 204 }), json(ledger))
    expect(await store.remove(2)).toBe(true)
    expect(mock).toHaveBeenCalledTimes(2)

    const refused = stubFetch(problem('Transaction is settled.', 409))
    expect(await store.remove(2)).toBe(false)
    expect(refused).toHaveBeenCalledTimes(1)
  })
})

describe('billableItemsStore', () => {
  it('fetchAll fills the items and passes the type filter', async () => {
    const mock = stubFetch(json({ items: [{ id: 1 }], totalCount: 1 }))
    const store = useBillableItemsStore()

    await store.fetchAll('Recurring')
    expect(store.items).toEqual([{ id: 1 }])
    expect(callOf(mock).url).toBe('/api/billing/billable-items?page=1&pageSize=100&type=Recurring')
  })

  it('fetchAll shows the API sentence on refusal', async () => {
    stubFetch(problem('Not allowed.', 403))
    const store = useBillableItemsStore()

    await store.fetchAll()
    expect(store.error).toBe('Not allowed.')
  })

  it('removeItem reloads on success and reports the refusal otherwise', async () => {
    const store = useBillableItemsStore()
    const mock = stubFetch(new Response(null, { status: 204 }), json({ items: [], totalCount: 0 }))
    expect(await store.removeItem(4)).toBe(true)
    expect(callOf(mock, 0)).toMatchObject({ url: '/api/billing/billable-items/4', method: 'DELETE' })
    expect(mock).toHaveBeenCalledTimes(2)

    stubFetch(problem('Already invoiced.', 409))
    expect(await store.removeItem(4)).toBe(false)
    expect(store.error).toBe('Already invoiced.')
  })
})

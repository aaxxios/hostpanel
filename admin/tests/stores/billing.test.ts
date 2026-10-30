/**
 * Tests for the invoices store: loads fill the lists, mutations reload the open invoice, and
 * a refusal, a missing answer or a field error lands in the store state instead of a view.
 */

import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { useBillingStore } from '@/stores/billing'
import { InvoiceRefundType } from '@/types/invoice'
import { json, problem, stubFetch, stubOffline, callOf } from '../helpers/fetch'

/** Sentence the client writes when nothing answered. */
const NO_ANSWER = 'Could not reach the server. Check your connection and try again.'

beforeEach(() => {
  setActivePinia(createPinia())
})

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('billingStore loads', () => {
  it('fetchAll fills the list and the total, sending only set filters', async () => {
    const mock = stubFetch(json({ items: [{ id: 1 }], pageNumber: 1, pageSize: 25, totalCount: 7, totalPages: 1 }))
    const store = useBillingStore()

    await store.fetchAll(1, 25, 'Paid')
    expect(store.invoices).toEqual([{ id: 1 }])
    expect(store.totalCount).toBe(7)
    expect(store.loading).toBe(false)
    expect(callOf(mock).url).toBe('/api/billing?page=1&pageSize=25&status=Paid')
  })

  it('fetchAll keeps the API sentence on refusal', async () => {
    stubFetch(problem('Billing is disabled.', 403))
    const store = useBillingStore()

    await store.fetchAll()
    expect(store.error).toBe('Billing is disabled.')
    expect(store.invoices).toEqual([])
  })

  it('fetchAll reports the no-answer sentence when offline', async () => {
    stubOffline()
    const store = useBillingStore()

    await store.fetchAll()
    expect(store.error).toBe(NO_ANSWER)
  })

  it('fetchById sets the open invoice and fetchClientInvoices fills the client list', async () => {
    const store = useBillingStore()
    stubFetch(json({ id: 5 }))
    await store.fetchById(5)
    expect(store.currentInvoice).toEqual({ id: 5 })

    const mock = stubFetch(json({ items: [{ id: 2 }], totalCount: 1 }))
    await store.fetchClientInvoices(9)
    expect(store.clientInvoices).toEqual([{ id: 2 }])
    expect(store.clientInvoicesTotal).toBe(1)
    expect(callOf(mock).url).toBe('/api/billing/client/9?page=1&pageSize=25')
  })
})

describe('billingStore mutations', () => {
  it('createInvoice returns the new id and null with a field error on refusal', async () => {
    const store = useBillingStore()
    stubFetch(json({ id: 9 }))
    expect(await store.createInvoice(1, '2026-02-01', [], false)).toBe(9)

    stubFetch(json({ type: 'VALIDATION_ERROR', title: 'Invalid', status: 400, detail: 'Fix the form.', errors: { DueDate: ['Due date is in the past.'] } }, 400))
    expect(await store.createInvoice(1, '2020-01-01', [], false)).toBeNull()
    expect(store.error).toBe('Fix the form.')
    expect(store.fieldErrors).toEqual({ dueDate: 'Due date is in the past.' })
  })

  it('publishInvoice sends the change then reloads the invoice', async () => {
    const mock = stubFetch(new Response(null, { status: 204 }), json({ id: 5, status: 'Unpaid' }))
    const store = useBillingStore()

    expect(await store.publishInvoice(5)).toBe(true)
    expect(callOf(mock, 0)).toMatchObject({ url: '/api/billing/5/publish', method: 'POST' })
    expect(callOf(mock, 1).url).toBe('/api/billing/5')
    expect(store.currentInvoice).toEqual({ id: 5, status: 'Unpaid' })
  })

  it('a refused mutation returns false, shows the sentence and does not reload', async () => {
    const mock = stubFetch(problem('Invoice already paid.',

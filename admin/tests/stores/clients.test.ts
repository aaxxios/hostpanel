/**
 * Tests for the clients store profile save: success refreshes the detail, a refusal lands in
 * `saveError` and the action reports failure as a value.
 */

import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { useClientsStore } from '@/stores/clients'
import { json, problem, stubFetch, stubOffline, callOf } from '../helpers/fetch'
import { installStoreHooks, NO_ANSWER } from '../helpers/storeHooks'
import { toUpdateClientPayload } from '@/lib/clientPayload'
import type { ClientDetail } from '@/types/client'

/** A loaded client carrying every group, the way the detail endpoint answers. */
const DETAIL = {
  id: 7,
  firstName: 'Ada',
  lastName: 'Lovelace',
  address: {},
  preferences: {},
  notifications: { general: true, invoice: true, support: true, product: true, domain: true, affiliate: true },
  settings: {
    overdueNotices: true,
    taxExempt: true,
    separateInvoices: false,
    disableCcProcessing: false,
    marketingOptIn: false,
    statusUpdate: true,
    allowSso: true,
  },
} as unknown as ClientDetail

/** A complete update payload, built the way a screen builds one from a loaded client. */
const PAYLOAD = toUpdateClientPayload(DETAIL, {})

describe('clientsStore.updateClient', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('returns true, re-reads the detail in place and leaves saveError null', async () => {
    stubFetch(json({ id: 1 }))
    const store = useClientsStore()

    expect(await store.updateClient(1, PAYLOAD)).toBe(true)
    expect(store.saving).toBe(false)
    expect(store.saveError).toBeNull()
    expect(store.current?.id).toBe(1)
    // The detail is never blanked, or the client layout would swap in its loading state.
    expect(store.loading).toBe(false)
  })

  it('returns false with the API detail on refusal', async () => {
    const body = { type: 'VALIDATION_ERROR', title: 'Invalid', status: 400, detail: 'Email is taken.' }
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(json(body, 400)))
    const store = useClientsStore()

    expect(await store.updateClient(1, PAYLOAD)).toBe(false)
    expect(store.saving).toBe(false)
    expect(store.saveError).toBe('Email is taken.')
  })

  it('returns false with the fallback when nothing answered', async () => {
    vi.stubGlobal('fetch', vi.fn().mockRejectedValue(new TypeError('offline')))
    const store = useClientsStore()

    expect(await store.updateClient(1, PAYLOAD)).toBe(false)
    expect(store.saveError).toBe('Could not reach the server. Check your connection and try again.')
  })
})

describe('clientsStore.saveAdminNotes', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('sends the whole record with only the notes replaced', async () => {
    const mock = stubFetch(json({ id: 7 }))
    const store = useClientsStore()
    store.current = DETAIL

    expect(await store.saveAdminNotes(7, '  call back  ')).toBe(true)

    const put = callOf(mock, 0)
    expect(put.method).toBe('PUT')
    expect(put.url).toBe('/api/clients/7')
    expect(put.body).toMatchObject({
      firstName: 'Ada',
      lastName: 'Lovelace',
      notifications: { invoice: true },
      settings: { taxExempt: true },
      preferences: { adminNotes: 'call back' },
    })
  })

  it('clears the notes when the text is empty', async () => {
    const mock = stubFetch(json({ id: 7 }))
    const store = useClientsStore()
    store.current = DETAIL

    await store.saveAdminNotes(7, '   ')

    expect(callOf(mock, 0).body).toMatchObject({ preferences: { adminNotes: null } })
  })

  it('does not call the API for a client that is not the open one', async () => {
    const mock = stubFetch(json({}))
    const store = useClientsStore()
    store.current = DETAIL

    expect(await store.saveAdminNotes(8, 'note')).toBe(false)
    expect(mock).not.toHaveBeenCalled()
  })
})

describe('clientsStore loads', () => {
  installStoreHooks()

  it('fetchAll fills the list from the store paging and the filters', async () => {
    const mock = stubFetch(json({ items: [{ id: 1 }], totalCount: 9 }))
    const store = useClientsStore()

    await store.fetchAll({ search: 'acme', email: '' })
    expect(store.clients).toEqual([{ id: 1 }])
    expect(store.totalCount).toBe(9)
    expect(store.loading).toBe(false)
    expect(callOf(mock).url).toBe('/api/clients?page=1&pageSize=20&search=acme')
  })

  it('fetchAll shows the API sentence on refusal and the no-answer sentence offline', async () => {
    const store = useClientsStore()
    stubFetch(problem('Clients are hidden.', 403))
    await store.fetchAll()
    expect(store.error).toBe('Clients are hidden.')

    stubOffline()
    await store.fetchAll()
    expect(store.error).toBe(NO_ANSWER)
  })

  it('fetchById sets the open client and clears it first', async () => {
    const store = useClientsStore()
    store.current = { id: 99 } as never
    stubFetch(json({ id: 4 }))

    await store.fetchById(4)
    expect(store.current).toEqual({ id: 4 })

    stubFetch(problem('Client not found.', 404))
    await store.fetchById(5)
    expect(store.current).toBeNull()
    expect(store.error).toBe('Client not found.')
  })

  it('createClient keeps the field errors of a refused form', async () => {
    stubFetch(json({ type: 'VALIDATION_ERROR', title: 'Invalid', status: 400, detail: 'Fix the form.', errors: { Email: ['Email is taken.'] } }, 400))
    const store = useClientsStore()

    expect(await store.createClient({} as never)).toBe(false)
    expect(store.saveError).toBe('Fix the form.')
    expect(store.fieldErrors).toEqual({ email: 'Email is taken.' })
  })

  it('fetchCountries caches the list and stays silent on failure', async () => {
    const store = useClientsStore()
    const mock = stubFetch(json([{ code: 'AM', name: 'Armenia' }]))
    await store.fetchCountries()
    await store.fetchCountries()
    expect(store.countries).toHaveLength(1)
    expect(mock).toHaveBeenCalledTimes(1)

    const other = useClientsStore()
    other.countries = []
    stubOffline()
    await other.fetchCountries()
    expect(other.countries).toEqual([])
    expect(other.error).toBeNull()
  })

  it('fetchCurrencies keeps only the enabled currencies and maps the symbol', async () => {
    stubFetch(json([
      { code: 'USD', prefix: '$', suffix: '', isEnabled: true },
      { code: 'EUR', prefix: '', suffix: ' EUR', isEnabled: true },
      { code: 'GBP', prefix: '', suffix: '', isEnabled: false },
    ]))
    const store = useClientsStore()

    await store.fetchCurrencies()
    expect(store.currencies).toEqual([
      { code: 'USD', name: 'USD', symbol: '$' },
      { code: 'EUR', name: 'EUR', symbol: ' EUR' },
    ])
  })
})

describe('clientsStore actions', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('createClient refreshes the list and returns true', async () => {
    const fetchMock = stubFetch(json({}), json({ items: [], totalCount: 0 }))
    const store = useClientsStore()

    expect(await store.createClient({} as never)).toBe(true)
    expect(fetchMock).toHaveBeenCalledTimes(2)
    expect(store.saving).toBe(false)
  })

  it('createClient returns false with the API sentence and does not refresh', async () => {
    const fetchMock = stubFetch(problem('Email already registered.'))
    const store = useClientsStore()

    expect(await store.createClient({} as never)).toBe(false)
    expect(store.saveError).toBe('Email already registered.')
    expect(fetchMock).toHaveBeenCalledTimes(1)
  })

  it('fetchClientOptions composes the display name with the company', async () => {
    stubFetch(json({
      items: [
        { id: 1, firstName: 'A', lastName: 'B', companyName: 'Acme', email: 'a@b.c', status: 'Active' },
        { id: 2, firstName: 'C', lastName: 'D', email: 'c@d.e', status: 'Inactive' },
      ],
      totalCount: 2,
    }))
    const store = useClientsStore()

    const options = await store.fetchClientOptions()
    expect(options.map(o => o.name)).toEqual(['A B (Acme)', 'C D'])
  })

  it('fetchClientOptions returns [] and sets error on refusal', async () => {
    stubFetch(problem('Forbidden.', 403))
    const store = useClientsStore()

    expect(await store.fetchClientOptions()).toEqual([])
    expect(store.error).toBe('Forbidden.')
  })

  it('searchClients and searchUsers are silent on failure', async () => {
    stubOffline()
    const store = useClientsStore()

    expect(await store.searchClients('a')).toEqual([])
    expect(await store.searchUsers('a')).toEqual([])
    expect(store.error).toBeNull()
  })

  it('searchUsers returns the matching items', async () => {
    stubFetch(json({ items: [{ id: 'u1', email: 'a@b.c', firstName: 'A', lastName: 'B' }], totalCount: 1 }))
    const store = useClientsStore()

    expect(await store.searchUsers('a')).toHaveLength(1)
  })

  it('addContact refreshes the client; a refusal lands in contactError', async () => {
    const store = useClientsStore()
    stubFetch(json({}), json({ id: 1 }))
    expect(await store.addContact(1, {})).toBe(true)
    expect(store.current).toEqual({ id: 1 })

    stubFetch(problem('Contact email is invalid.'))
    expect(await store.updateContact(1, 5, {})).toBe(false)
    expect(store.contactError).toBe('Contact email is invalid.')
    expect(await store.removeContact(1, 5)).toBe(false)
  })

  it('exportClient returns the document, null with exportError on refusal', async () => {
    const store = useClientsStore()
    const mock = stubFetch(json({ profile: {} }))
    expect(await store.exportClient(1, ['profileData', 'notes'])).toEqual({ profile: {} })
    expect(mock.mock.calls[0]![0]).toBe('/api/clients/1/export?fields=profileData&fields=notes')

    stubFetch(problem('Export is disabled.'))
    expect(await store.exportClient(1, [])).toBeNull()
    expect(store.exportError).toBe('Export is disabled.')
  })
})

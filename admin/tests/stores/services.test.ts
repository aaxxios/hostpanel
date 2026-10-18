/**
 * Tests for the services store loads and typed writes: a load puts the API's sentence into
 * `error`, a mutation into `actionError`, a refused save exposes the field messages, and the
 * request bodies match what the API expects. The requests go through the real `useApi` transport.
 */

import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { translate } from '@/lib/translate'
import { useServicesStore } from '@/stores/services'
import { json, problem, stubFetch, stubOffline, callOf, invalid } from '../helpers/fetch'

describe('servicesStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('fetchAll requests the current page and fills the list', async () => {
    const mock = stubFetch(json({ items: [{ id: 1 }], totalCount: 9 }))
    const store = useServicesStore()
    store.page = 3

    expect(await store.fetchAll()).toBe(true)
    expect(callOf(mock).url).toBe('/api/services?page=3&pageSize=20')
    expect(store.services).toEqual([{ id: 1 }])
    expect(store.totalCount).toBe(9)
    expect(store.loading).toBe(false)
  })

  it('fetchAll reports the API sentence, and the no-answer sentence offline', async () => {
    const store = useServicesStore()
    stubFetch(problem('Not allowed.', 403))
    expect(await store.fetchAll()).toBe(false)
    expect(store.error).toBe('Not allowed.')

    stubOffline()
    expect(await store.fetchAll()).toBe(false)
    expect(store.error).toBe(translate('errors.noAnswer'))
  })

  it('fetchClientPage and fetchClientServices filter by client', async () => {
    const mock = stubFetch(json({ items: [{ id: 5 }], totalCount: 1 }))
    const store = useServicesStore()

    expect(await store.fetchClientPage(7, 2, 10)).toEqual({ items: [{ id: 5 }], totalCount: 1 })
    expect(await store.fetchClientServices(7)).toEqual([{ id: 5 }])
    expect(callOf(mock, 0).url).toBe('/api/services?clientId=7&page=2&pageSize=10')
    expect(callOf(mock, 1).url).toBe('/api/services?clientId=7&page=1&pageSize=100')
    expect(store.services).toEqual([])
  })

  it('a refused save returns false with the sentence and the field messages', async () => {
    const store = useServicesStore()
    stubFetch(invalid('Check the form.', { Quantity: ['Must be at least 1.'] }))

    expect(await store.update(4, {
      domain: null, dedicatedIp: null, username: null, password: null, provisioningRef: null,
      recurringAmount: 0, firstPaymentAmount: 0, nextRenewalAt: null, createdAt: null, terminatedAt: null,
      billingCycle: 'monthly', paymentMethod: null, promotionCode: null, subscriptionId: null,
      overrideAutoSuspend: false, suspendUntil: null, autoTerminateEndOfCycle: false, autoTerminateReason: null,
      adminNotes: null, status: 'Active', quantity: 0, serverId: null, productId: null,
    })).toBe(false)
    expect(store.actionError).toBe('Check the form.')
    expect(store.fieldErrors).toEqual({ quantity: 'Must be at least 1.' })

    stubFetch(new Response(null, { status: 204 }))
    expect(await store.unsuspend(4)).toBe(true)
    expect(store.actionError).toBeNull()
    expect(store.fieldErrors).toEqual({})
  })

  it('password and package changes send their bodies', async () => {
    const store = useServicesStore()
    const mock = stubFetch(new Response(null, { status: 204 }))

    expect(await store.changePassword(4, 'secret')).toBe(true)
    expect(await store.changePackage(4, 'Gold')).toBe(true)
    expect(await store.provision(4)).toBe(true)
    expect(callOf(mock, 0)).toMatchObject({ url: '/api/provisioning/4/change-password', body: { newPassword: 'secret' } })
    expect(callOf(mock, 1)).toMatchObject({ url: '/api/provisioning/4/change-package', body: { newPackage: 'Gold' } })
    expect(callOf(mock, 2)).toMatchObject({ url: '/api/provisioning/4/provision', method: 'POST' })
  })

  it('fetchById stores the detail in current', async () => {
    stubFetch(json({ id: 4 }))
    const store = useServicesStore()

    expect(await store.fetchById(4)).toBe(true)
    expect(store.current).toEqual({ id: 4 })
  })

  it('fetchById reports a refusal through error', async () => {
    stubFetch(problem('Service not found.', 404))
    const store = useServicesStore()

    expect(await store.fetchById(4)).toBe(false)
    expect(store.error).toBe('Service not found.')
  })

  it('fetchClientServices is silent on failure', async () => {
    stubOffline()
    const store = useServicesStore()

    expect(await store.fetchClientServices(7)).toEqual([])
    expect(store.error).toBeNull()
  })

  it('provisioning commands send the reason and report refusals', async () => {
    const mock = stubFetch(json({}))
    const store = useServicesStore()

    expect(await store.suspend(4, 'Admin suspended')).toBe(true)
    expect(JSON.parse(mock.mock.calls[0]![1].body as string)).toEqual({ reason: 'Admin suspended' })

    stubFetch(problem('Panel unreachable.', 502))
    expect(await store.terminate(4, 'Admin terminated')).toBe(false)
    expect(store.actionError).toBe('Panel unreachable.')
  })

  it('fetchPanelLoginUrl returns the url, null when refused', async () => {
    const store = useServicesStore()
    stubFetch(json({ url: 'https://panel.example/login' }))
    expect(await store.fetchPanelLoginUrl(4)).toBe('https://panel.example/login')

    stubFetch(problem('No panel.', 400))
    expect(await store.fetchPanelLoginUrl(4)).toBeNull()
    expect(store.actionError).toBe('No panel.')
  })
})

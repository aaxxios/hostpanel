/**
 * Tests for the services API composable: paths, verbs, query strings and bodies through the
 * real `useApi` transport with `fetch` stubbed.
 */

import { afterEach, describe, expect, it, vi } from 'vitest'
import { useServicesApi } from '@/composables/apis/useServicesApi'
import type { UpdateServicePayload } from '@/types/service'
import { json, stubFetch, callOf } from '../../helpers/fetch'

afterEach(() => {
  vi.unstubAllGlobals()
})

/** A valid service save body. */
const fields: UpdateServicePayload = {
  domain: 'example.com',
  dedicatedIp: null,
  username: null,
  password: null,
  provisioningRef: null,
  recurringAmount: 12,
  firstPaymentAmount: 12,
  nextRenewalAt: null,
  createdAt: null,
  terminatedAt: null,
  billingCycle: 'monthly',
  paymentMethod: null,
  promotionCode: null,
  subscriptionId: null,
  overrideAutoSuspend: false,
  suspendUntil: null,
  autoTerminateEndOfCycle: false,
  autoTerminateReason: null,
  adminNotes: null,
  status: 'Active',
  quantity: 1,
  serverId: null,
  productId: 2,
}

describe('useServicesApi', () => {
  it('listServices sends the client only when given', async () => {
    const mock = stubFetch(json({ items: [], totalCount: 0 }))
    const api = useServicesApi()

    await api.listServices({ page: 1, pageSize: 20 })
    await api.listServices({ clientId: 7, page: 2, pageSize: 100 })
    expect(callOf(mock, 0).url).toBe('/api/services?page=1&pageSize=20')
    expect(callOf(mock, 1).url).toBe('/api/services?clientId=7&page=2&pageSize=100')
  })

  it('getService reads the detail and updateService PUTs the fields', async () => {
    const mock = stubFetch(json({ id: 4 }))
    const api = useServicesApi()

    expect(await api.getService(4)).toEqual({ id: 4 })
    await api.updateService(4, fields)
    expect(callOf(mock, 0).url).toBe('/api/services/4')
    expect(callOf(mock, 1)).toMatchObject({ url: '/api/services/4', method: 'PUT', body: fields })
  })

  it('getPanelLoginUrl reads the single sign-on url', async () => {
    const mock = stubFetch(json({ url: 'https://panel.example/login' }))

    expect(await useServicesApi().getPanelLoginUrl(4)).toEqual({ url: 'https://panel.example/login' })
    expect(callOf(mock).url).toBe('/api/provisioning/4/cpanel-sso')
  })

  it('provisioning commands POST to /provisioning with their bodies', async () => {
    const mock = stubFetch(new Response(null, { status: 204 }))
    const api = useServicesApi()

    await api.provisionService(4)
    await api.changePassword(4, 'secret')
    await api.changePackage(4, 'Gold')
    await api.suspendService(4, { reason: 'Abuse' })
    await api.unsuspendService(4)
    await api.terminateService(4, { reason: 'Requested' })
    expect(callOf(mock, 0)).toMatchObject({ url: '/api/provisioning/4/provision', method: 'POST' })
    expect(callOf(mock, 1)).toMatchObject({ url: '/api/provisioning/4/change-password', body: { newPassword: 'secret' } })
    expect(callOf(mock, 2)).toMatchObject({ url: '/api/provisioning/4/change-package', body: { newPackage: 'Gold' } })
    expect(callOf(mock, 3)).toMatchObject({ url: '/api/provisioning/4/suspend', body: { reason: 'Abuse' } })
    expect(callOf(mock, 4)).toMatchObject({ url: '/api/provisioning/4/unsuspend', method: 'POST' })
    expect(callOf(mock, 5)).toMatchObject({ url: '/api/provisioning/4/terminate', body: { reason: 'Requested' } })
  })
})

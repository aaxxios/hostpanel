/**
 * Tests for the client users API composable: the users linked to a client and the Identity
 * user search, through the real `useApi` transport with `fetch` stubbed.
 */

import { afterEach, describe, expect, it, vi } from 'vitest'
import { useClientUsersApi } from '@/composables/apis/useClientUsersApi'
import { ApiError } from '@/composables/useApi'
import { json, problem, stubFetch, callOf } from '../../helpers/fetch'

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('useClientUsersApi', () => {
  it('list reads the users of the client', async () => {
    const mock = stubFetch(json([{ userId: 'u1' }]))

    expect(await useClientUsersApi().list('7')).toEqual([{ userId: 'u1' }])
    expect(callOf(mock).url).toBe('/api/clients/7/users')
  })

  it('search asks the admin users endpoint for the first page', async () => {
    const mock = stubFetch(json({ items: [] }))

    await useClientUsersApi().search('ann', 5)
    expect(callOf(mock).url).toBe('/api/admin/users?search=ann&pageSize=5&page=1')
  })

  it('add, invite and updatePermissions send their bodies', async () => {
    const mock = stubFetch(new Response(null, { status: 204 }))
    const api = useClientUsersApi()

    await api.add('7', 'u1', 3)
    await api.invite('7', { email: 'a@b.c' })
    await api.updatePermissions('7', 'u1', 5)
    expect(callOf(mock, 0)).toMatchObject({ url: '/api/clients/7/users', method: 'POST', body: { memberUserId: 'u1', permissions: 3 } })
    expect(callOf(mock, 1)).toMatchObject({ url: '/api/clients/7/users/invite', method: 'POST', body: { email: 'a@b.c' } })
    expect(callOf(mock, 2)).toMatchObject({ url: '/api/clients/7/users/u1/permissions', method: 'PUT', body: { permissions: 5 } })
  })

  it('makeOwner POSTs and remove DELETEs', async () => {
    const mock = stubFetch(new Response(null, { status: 204 }))
    const api = useClientUsersApi()

    await api.makeOwner('7', 'u1')
    await api.remove('7', 'u1')
    expect(callOf(mock, 0)).toMatchObject({ url: '/api/clients/7/users/u1/make-owner', method: 'POST' })
    expect(callOf(mock, 1)).toMatchObject({ url: '/api/clients/7/users/u1', method: 'DELETE' })
  })

  it('rejects with an ApiError carrying the refusal body', async () => {
    stubFetch(problem('Owner cannot be removed.', 409))

    await expect(useClientUsersApi().remove('7', 'u1')).rejects.toBeInstanceOf(ApiError)
  })
})

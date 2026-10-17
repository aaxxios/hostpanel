/**
 * Tests for the admin users API composable: paths, verbs and bodies through the real
 * `useApi` transport with `fetch` stubbed.
 */

import { afterEach, describe, expect, it, vi } from 'vitest'
import { useUsersApi } from '@/composables/apis/useUsersApi'
import { ApiError } from '@/composables/useApi'
import { json, problem, stubFetch, callOf } from '../../helpers/fetch'

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('useUsersApi', () => {
  it('list drops an empty search and sends a set one', async () => {
    const mock = stubFetch(json({ items: [], totalCount: 0 }))
    const api = useUsersApi()

    await api.list(1, 20)
    await api.list(1, 20, 'ann')
    expect(callOf(mock, 0).url).toBe('/api/admin/users?page=1&pageSize=20')
    expect(callOf(mock, 1).url).toBe('/api/admin/users?page=1&pageSize=20&search=ann')
  })

  it('get reads one user', async () => {
    const mock = stubFetch(json({ id: 'u1' }))

    expect(await useUsersApi().get('u1')).toEqual({ id: 'u1' })
    expect(callOf(mock).url).toBe('/api/admin/users/u1')
  })

  it('update, remove, sendPasswordReset and changePassword use the user paths', async () => {
    const mock = stubFetch(new Response(null, { status: 204 }))
    const api = useUsersApi()
    const profile = { firstName: 'A', lastName: 'B', email: 'a@b.c', language: null }

    await api.update('u1', profile)
    await api.remove('u1')
    await api.sendPasswordReset('u1')
    await api.changePassword('u1', 'secret')
    expect(callOf(mock, 0)).toMatchObject({ url: '/api/admin/users/u1', method: 'PUT', body: profile })
    expect(callOf(mock, 1)).toMatchObject({ url: '/api/admin/users/u1', method: 'DELETE' })
    expect(callOf(mock, 2)).toMatchObject({ url: '/api/admin/users/u1/reset-password', method: 'POST' })
    expect(callOf(mock, 3)).toMatchObject({ url: '/api/admin/users/u1/change-password', method: 'POST', body: { password: 'secret' } })
  })

  it('rejects with an ApiError carrying the refusal body', async () => {
    stubFetch(problem('User owns a client.', 409))

    await expect(useUsersApi().remove('u1')).rejects.toBeInstanceOf(ApiError)
  })
})

/**
 * Tests for the servers API composable: paths, verbs and bodies through the real `useApi`
 * transport with `fetch` stubbed.
 */

import { afterEach, describe, expect, it, vi } from 'vitest'
import { useServersApi } from '@/composables/apis/useServersApi'
import { json, problem, stubFetch, callOf } from '../../helpers/fetch'

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('useServersApi servers', () => {
  it('listServers reads /admin/servers', async () => {
    const mock = stubFetch(json([{ id: 1 }]))

    expect(await useServersApi().listServers()).toEqual([{ id: 1 }])
    expect(callOf(mock)).toMatchObject({ url: '/api/admin/servers', method: 'GET' })
  })

  it('createServer, updateServer and deleteServer use POST, PUT and DELETE', async () => {
    const mock = stubFetch(new Response(null, { status: 204 }))
    const api = useServersApi()

    await api.createServer({ name: 'a' } as never)
    await api.updateServer(2, { name: 'b' } as never)
    await api.deleteServer(2)
    expect(callOf(mock, 0)).toMatchObject({ url: '/api/admin/servers', method: 'POST', body: { name: 'a' } })
    expect(callOf(mock, 1)).toMatchObject({ url: '/api/admin/servers/2', method: 'PUT', body: { name: 'b' } })
    expect(callOf(mock, 2)).toMatchObject({ url: '/api/admin/servers/2', method: 'DELETE' })
  })

  it('testServerConnection POSTs to test-connection and returns the result', async () => {
    const mock = stubFetch(json({ success: true }))

    expect(await useServersApi().testServerConnection(2)).toEqual({ success: true })
    expect(callOf(mock)).toMatchObject({ url: '/api/admin/servers/2/test-connection', method: 'POST' })
  })

  it('rejects when the API refuses', async () => {
    stubFetch(problem('Server has accounts.', 409))

    await expect(useServersApi().deleteServer(1)).rejects.toBeTruthy()
  })
})

describe('useServersApi groups', () => {
  it('listServerGroups reads /admin/server-groups', async () => {
    const mock = stubFetch(json([]))

    await useServersApi().listServerGroups()
    expect(callOf(mock)).toMatchObject({ url: '/api/admin/server-groups', method: 'GET' })
  })

  it('createServerGroup, updateServerGroup and deleteServerGroup use POST, PUT and DELETE', async () => {
    const mock = stubFetch(new Response(null, { status: 204 }))
    const api = useServersApi()

    await api.createServerGroup({ name: 'g' } as never)
    await api.updateServerGroup(4, { name: 'h' } as never)
    await api.deleteServerGroup(4)
    expect(callOf(mock, 0)).toMatchObject({ url: '/api/admin/server-groups', method: 'POST', body: { name: 'g' } })
    expect(callOf(mock, 1)).toMatchObject({ url: '/api/admin/server-groups/4', method: 'PUT', body: { name: 'h' } })
    expect(callOf(mock, 2)).toMatchObject({ url: '/api/admin/server-groups/4', method: 'DELETE' })
  })
})

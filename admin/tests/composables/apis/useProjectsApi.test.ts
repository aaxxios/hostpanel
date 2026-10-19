/**
 * Tests for the projects API composable: paths, verbs and bodies through the real `useApi`
 * transport with `fetch` stubbed.
 */

import { afterEach, describe, expect, it, vi } from 'vitest'
import { useProjectsApi } from '@/composables/apis/useProjectsApi'
import { callOf, json, problem, stubFetch } from '../../helpers/fetch'

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('useProjectsApi', () => {
  it('list reads /admin/projects', async () => {
    const mock = stubFetch(json([{ id: 1 }]))

    expect(await useProjectsApi().list()).toEqual([{ id: 1 }])
    expect(callOf(mock)).toMatchObject({ url: '/api/admin/projects', method: 'GET' })
  })

  it('create POSTs the payload and returns the id', async () => {
    const mock = stubFetch(json(7))

    expect(await useProjectsApi().create({ slug: 'site' } as never)).toBe(7)
    expect(callOf(mock)).toMatchObject({ url: '/api/admin/projects', method: 'POST', body: { slug: 'site' } })
  })

  it('update PUTs by id and remove DELETEs by id', async () => {
    const mock = stubFetch(new Response(null, { status: 204 }))
    const api = useProjectsApi()

    await api.update(2, { slug: 'site' } as never)
    await api.remove(2)
    expect(callOf(mock, 0)).toMatchObject({ url: '/api/admin/projects/2', method: 'PUT', body: { slug: 'site' } })
    expect(callOf(mock, 1)).toMatchObject({ url: '/api/admin/projects/2', method: 'DELETE' })
  })

  it('reorder PUTs the pairs wrapped in items', async () => {
    const mock = stubFetch(new Response(null, { status: 204 }))

    await useProjectsApi().reorder([{ id: 1, sortOrder: 0 }])
    expect(callOf(mock)).toMatchObject({
      url: '/api/admin/projects/order',
      method: 'PUT',
      body: { items: [{ id: 1, sortOrder: 0 }] },
    })
  })

  it('rejects when the API refuses', async () => {
    stubFetch(problem('Entry is locked.', 409))

    await expect(useProjectsApi().remove(1)).rejects.toBeTruthy()
  })
})

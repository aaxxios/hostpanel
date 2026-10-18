/**
 * Tests for the plugins API composable: paths, verbs and the multipart install through the real
 * `useApi` transport with `fetch` stubbed.
 */

import { afterEach, describe, expect, it, vi } from 'vitest'
import { usePluginsApi } from '@/composables/apis/usePluginsApi'
import { json, problem, stubFetch, callOf } from '../../helpers/fetch'

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('usePluginsApi', () => {
  it('listPlugins reads /admin/plugins', async () => {
    const mock = stubFetch(json([{ id: 'p' }]))

    expect(await usePluginsApi().listPlugins()).toEqual([{ id: 'p' }])
    expect(callOf(mock)).toMatchObject({ url: '/api/admin/plugins', method: 'GET' })
  })

  it('installPlugin posts the file as multipart form data', async () => {
    const mock = stubFetch(json({ requiresRestart: true }))
    const zip = new File(['z'], 'plugin.zip', { type: 'application/zip' })

    expect(await usePluginsApi().installPlugin(zip)).toEqual({ requiresRestart: true })
    const call = callOf(mock)
    expect(call).toMatchObject({ url: '/api/admin/plugins/install', method: 'POST' })
    expect((call.init.body as FormData).get('file')).toBeInstanceOf(File)
  })

  it('removePlugin DELETEs by id and restartServer POSTs to restart', async () => {
    const mock = stubFetch(json({ requiresRestart: true }))
    const api = usePluginsApi()

    expect(await api.removePlugin('p')).toEqual({ requiresRestart: true })
    await api.restartServer()
    expect(callOf(mock, 0)).toMatchObject({ url: '/api/admin/plugins/p', method: 'DELETE' })
    expect(callOf(mock, 1)).toMatchObject({ url: '/api/admin/plugins/restart', method: 'POST' })
  })

  it('rejects when the API refuses', async () => {
    stubFetch(problem('Not a plugin archive.', 400))

    await expect(usePluginsApi().installPlugin(new File(['x'], 'a.zip'))).rejects.toBeTruthy()
  })
})

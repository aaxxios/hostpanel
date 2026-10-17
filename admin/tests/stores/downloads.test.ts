/**
 * Tests for the downloads store: loads fill state, mutations report the outcome as a value, a
 * refusal puts the API's sentence into `error`, and a refused save exposes the field messages.
 * The requests go through the real `useApi` transport.
 */

import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { translate } from '@/lib/translate'
import { DownloadTarget, useDownloadsStore } from '@/stores/downloads'
import { json, problem, stubFetch, stubOffline, callOf, invalid } from '../helpers/fetch'
import { DownloadType } from '@/types/downloaditem'

/** A valid category body. */
const category = { name: 'Drivers', description: '', isHidden: false, parentCategoryId: null }

/** A valid download body. */
const download = {
  title: 'Setup',
  description: '',
  type: DownloadType.Zip,
  filename: 'setup.zip',
  categoryId: 3,
  clientsOnly: false,
  productDownload: false,
  isHidden: false,
}

describe('downloadsStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('fetchAll loads categories and the downloads of one category', async () => {
    const mock = stubFetch(json([{ id: 1 }]))
    const store = useDownloadsStore()

    expect(await store.fetchAll(4)).toBe(true)
    const urls = [callOf(mock, 0).url, callOf(mock, 1).url].sort()
    expect(urls).toEqual(['/api/downloads/categories', '/api/downloads?categoryId=4'])
    expect(store.categories).toEqual([{ id: 1 }])
    expect(store.downloads).toEqual([{ id: 1 }])
  })

  it('a refused load reports the API sentence, the no-answer sentence offline', async () => {
    const store = useDownloadsStore()
    stubFetch(problem('Not allowed.', 403))
    expect(await store.fetchCategories()).toBe(false)
    expect(store.error).toBe('Not allowed.')

    stubOffline()
    expect(await store.fetchDownloads(null)).toBe(false)
    expect(store.error).toBe(translate('errors.noAnswer'))
  })

  it('category and download writes send their payloads', async () => {
    const store = useDownloadsStore()
    const mock = stubFetch(new Response(null, { status: 204 }))

    expect(await store.createCategory(category)).toBe(true)
    expect(await store.updateCategory(3, category)).toBe(true)
    expect(await store.createDownload(download)).toBe(true)
    expect(await store.updateDownload(7, download)).toBe(true)
    expect(callOf(mock, 0)).toMatchObject({ url: '/api/downloads/categories', method: 'POST', body: category })
    expect(callOf(mock, 1)).toMatchObject({ url: '/api/downloads/categories/3', method: 'PUT', body: category })
    expect(callOf(mock, 2)).toMatchObject({ url: '/api/downloads', method: 'POST', body: download })
    expect(callOf(mock, 3)).toMatchObject({ url: '/api/downloads/7', method: 'PUT', body: download })
  })

  it('remove picks the endpoint by target', async () => {
    const store = useDownloadsStore()
    const mock = stubFetch(new Response(null, { status: 204 }))

    expect(await store.remove(DownloadTarget.Category, 3)).toBe(true)
    expect(await store.remove(DownloadTarget.Download, 7)).toBe(true)
    expect(callOf(mock, 0)).toMatchObject({ url: '/api/downloads/categories/3', method: 'DELETE' })
    expect(callOf(mock, 1)).toMatchObject({ url: '/api/downloads/7', method: 'DELETE' })
  })

  it('a refused save returns false with the sentence and the field messages', async () => {
    const store = useDownloadsStore()
    stubFetch(invalid('Check the form.', { Filename: ['Filename is required.'] }))

    expect(await store.createDownload({ ...download, filename: '' })).toBe(false)
    expect(store.error).toBe('Check the form.')
    expect(store.fieldErrors).toEqual({ filename: 'Filename is required.' })

    stubFetch(new Response(null, { status: 204 }))
    expect(await store.createDownload(download)).toBe(true)
    expect(store.error).toBeNull()
    expect(store.fieldErrors).toEqual({})
  })
})

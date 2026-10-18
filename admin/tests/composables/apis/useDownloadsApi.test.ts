/**
 * Tests for the downloads API composable: paths, verbs, query strings and bodies through the
 * real `useApi` transport with `fetch` stubbed.
 */

import { afterEach, describe, expect, it, vi } from 'vitest'
import { useDownloadsApi } from '@/composables/apis/useDownloadsApi'
import { json, stubFetch, callOf } from '../../helpers/fetch'
import { DownloadType } from '@/types/downloaditem'

afterEach(() => {
  vi.unstubAllGlobals()
})

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

describe('useDownloadsApi', () => {
  it('listCategories reads the category list', async () => {
    const mock = stubFetch(json([]))

    await useDownloadsApi().listCategories()
    expect(callOf(mock)).toMatchObject({ url: '/api/downloads/categories', method: 'GET' })
  })

  it('listDownloads filters by category and sends none for null or 0', async () => {
    const mock = stubFetch(json([]))
    const api = useDownloadsApi()

    await api.listDownloads(4)
    await api.listDownloads(null)
    await api.listDownloads(0)
    expect(callOf(mock, 0).url).toBe('/api/downloads?categoryId=4')
    expect(callOf(mock, 1).url).toBe('/api/downloads')
    expect(callOf(mock, 2).url).toBe('/api/downloads')
  })

  it('category writes hit /downloads/categories', async () => {
    const mock = stubFetch(new Response(null, { status: 204 }))
    const api = useDownloadsApi()

    await api.createCategory(category)
    await api.updateCategory(3, category)
    await api.deleteCategory(3)
    expect(callOf(mock, 0)).toMatchObject({ url: '/api/downloads/categories', method: 'POST', body: category })
    expect(callOf(mock, 1)).toMatchObject({ url: '/api/downloads/categories/3', method: 'PUT', body: category })
    expect(callOf(mock, 2)).toMatchObject({ url: '/api/downloads/categories/3', method: 'DELETE' })
  })

  it('download writes hit /downloads', async () => {
    const mock = stubFetch(new Response(null, { status: 204 }))
    const api = useDownloadsApi()

    await api.createDownload(download)
    await api.updateDownload(7, download)
    await api.deleteDownload(7)
    expect(callOf(mock, 0)).toMatchObject({ url: '/api/downloads', method: 'POST', body: download })
    expect(callOf(mock, 1)).toMatchObject({ url: '/api/downloads/7', method: 'PUT', body: download })
    expect(callOf(mock, 2)).toMatchObject({ url: '/api/downloads/7', method: 'DELETE' })
  })
})

/**
 * Tests for the upload API composable: both endpoints send the file as multipart form data under
 * the field `file` and answer the stored image's URL.
 */

import { afterEach, describe, expect, it, vi } from 'vitest'
import { useUploadApi } from '@/composables/apis/useUploadApi'
import { json, problem, stubFetch, callOf } from '../../helpers/fetch'

/** A file standing in for the picked image. */
const image = new File(['x'], 'a.png', { type: 'image/png' })

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('useUploadApi', () => {
  it('uploadBrandingImage posts to the endpoint of the kind', async () => {
    const mock = stubFetch(json({ url: '/img/logo.png' }))

    expect(await useUploadApi().uploadBrandingImage('logodark', image)).toEqual({ url: '/img/logo.png' })
    const call = callOf(mock)
    expect(call).toMatchObject({ url: '/api/admin/settings/branding/logodark', method: 'POST' })
    expect((call.init.body as FormData).get('file')).toBeInstanceOf(File)
  })

  it('uploadSlideImage posts to the slide endpoint', async () => {
    const mock = stubFetch(json({ url: '/img/s.png' }))

    expect(await useUploadApi().uploadSlideImage(image)).toEqual({ url: '/img/s.png' })
    expect(callOf(mock)).toMatchObject({ url: '/api/admin/slides/upload-image', method: 'POST' })
  })

  it('does not force a JSON content type, so the browser writes the multipart boundary', async () => {
    const mock = stubFetch(json({ url: '/x' }))

    await useUploadApi().uploadSlideImage(image)
    const headers = callOf(mock).init.headers as Record<string, string>
    expect(headers['Content-Type']).toBeUndefined()
  })

  it('rejects when the API refuses', async () => {
    stubFetch(problem('The file is too large.', 413))

    await expect(useUploadApi().uploadSlideImage(image)).rejects.toBeTruthy()
  })
})

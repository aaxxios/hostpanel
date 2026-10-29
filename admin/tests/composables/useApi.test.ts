/**
 * Tests for the admin `useApi` transport: every API call carries `X-Language`, read from the
 * active i18n locale at call time; queries, failures, the 401 probe, and `href` for browser-opened addresses.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { i18n } from '@/i18n'
import { MessageLanguage } from '@/locales/messages'
import { buildQuery, useApi } from '@/composables/useApi'
import { json } from '../helpers/fetch'

/**
 * Stubs `fetch` with a JSON answer and returns the stub for header inspection.
 *
 * @returns The fetch mock.
 */
const stubFetch = () => {
  const mock = vi.fn(async () => json({}))
  vi.stubGlobal('fetch', mock)
  return mock
}

/**
 * Reads the headers of the first recorded call.
 *
 * @param mock - The fetch mock.
 * @returns The headers sent.
 */
const sentHeaders = (mock: ReturnType<typeof stubFetch>): Record<string, string> =>
  (mock.mock.calls[0] as unknown as [string, RequestInit])[1].headers as Record<string, string>

afterEach(() => {
  i18n.global.locale.value = MessageLanguage.En
  vi.unstubAllGlobals()
})

describe('useApi language header', () => {
  it('sends X-Language from the current locale on a request', async () => {
    const mock = stubFetch()
    i18n.global.locale.value = MessageLanguage.Ru

    await useApi().request('/admin/dashboard/stats')

    expect(sentHeaders(mock)['X-Language']).toBe('ru')
  })

  it('follows a locale switch on the next request', async () => {
    const mock = stubFetch()
    i18n.global.locale.value = MessageLanguage.Hy

    await useApi().request('/admin/dashboard/stats')

    expect(sentHeaders(mock)['X-Language']).toBe('hy')
  })

  it('sends X-Language on an upload', async () => {
    const mock = stubFetch()
    i18n.global.locale.value = MessageLanguage.Ru

    await useApi().upload('/admin/storefront/upload-image', new File(['x'], 'a.png'))

    expect(sentHeaders(mock)['X-Language']).toBe('ru')
  })
})

describe('buildQuery', () => {
  it('drops undefined, null and empty values', () => {
    expect(buildQuery({ a: 'x', b: undefined, c: null, d: '' })).toBe('?a=x')
  })

  it('keeps zero and false', () => {
    expect(buildQuery({ page: 0, active: false })).toBe('?page=0&active=false')
  })

  it('repeats the key for an array', () => {
    expect(buildQuery({ id: [1, 2], q: 'a b' })).toBe('?id=1&id=2&q=a+b')
  })

  it('answers an empty string when nothing is left', () => {
    expect(buildQuery({ a: undefined, b: '' })).toBe('')
  })
})

describe('useApi failures', () => {
  it('throws an ApiError that keeps the problem body', async () => {
    vi.stubGlobal('fetch', vi.fn(async () => json({ type: 'REFUSED', detail: 'No.' }, 403)))

    await expect(useApi().request('/x')).rejects.toMatchObject({ status: 403, body: { detail: 'No.' } })
  })

  it('throws an ApiError with a null body when the answer was not JSON', async () => {
    vi.stubGlobal('fetch', vi.fn(async () => new Response('<html>', { status: 502 })))

    await expect(useApi().request('/x')).rejects.toMatchObject({ status: 502, body: null })
  })
})

describe('useApi probe', () => {
  it('resolves null on a 401 without throwing or logging', async () => {
    vi.stubGlobal('fetch', vi.fn(async () => json({ detail: 'Sign in.' }, 401)))
    const logged = vi.spyOn(console, 'error').mockImplementation(() => undefined)

    await expect(useApi().probe('/auth/me')).resolves.toBeNull()
    expect(logged).not.toHaveBeenCalled()
    logged.mockRestore()
  })

  it('returns the parsed body on success', async () => {
    vi.stubGlobal('fetch', vi.fn(async () => json({ email: 'a@b.c' })))

    await expect(useApi().probe('/auth/me')).resolves.toEqual({ email: 'a@b.c' })
  })

  it('still throws for any other failure', async () => {
    vi.stubGlobal('fetch', vi.fn(async () => json({ detail: 'Boom.' }, 500)))

    await expect(useApi().probe('/auth/me')).rejects.toMatchObject({ status: 500 })
  })
})

describe('useApi href', () => {
  it('builds the address of an endpoint the browser opens itself, without a request', () => {
    const fetchSpy = vi.fn()
    vi.stubGlobal('fetch', fetchSpy)

    expect(useApi().href('/billing/5/pdf')).toBe('/api/billing/5/pdf')
    expect(fetchSpy).not.toHaveBeenCalled()
  })
})

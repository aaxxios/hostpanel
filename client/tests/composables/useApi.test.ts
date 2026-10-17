/**
 * Tests for the browser-side transport in `composables/useApi.ts`.
 *
 * The language proof starts here: every call must stamp `X-Language` (never the retired
 * `x-locale`) from the active i18n locale, validated against `en|ru|hy`. `useApi` is checked at
 * the `useFetch` boundary it wraps (a framework function, not ours); `apiFetch` is checked
 * against a real HTTP stub server, asserting on what actually arrived.
 *
 * @module tests/composables/useApi.test
 */

import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { ref, toValue } from 'vue'
import { ofetch } from 'ofetch'
import { startStubServer } from '../helpers/stubServer'
import type { StubServer } from '../helpers/stubServer'

/** The active locale the stubbed i18n instance reports; tests change it to simulate a switch. */
const locale = ref('en')

/** Captures the options `useApi` hands to `useFetch`. */
const useFetchMock = vi.fn((_url: unknown, _options: Record<string, unknown>) => ({}))

/** The auth-guard hook a 401 must reach. */
const handleAuthExpired = vi.fn(async () => undefined)

/** The stub started by the current test. */
let stub: StubServer | undefined

vi.stubGlobal('toValue', toValue)
vi.stubGlobal('useFetch', useFetchMock)

const { useApi, apiFetch } = await import('~/composables/useApi')

beforeEach(() => {
  locale.value = 'en'
  useFetchMock.mockClear()
  handleAuthExpired.mockClear()
  vi.stubGlobal('useI18n', () => ({ locale }))
})

afterEach(async () => {
  await stub?.close()
  stub = undefined
})

/**
 * Reads the header map `useApi` passed to `useFetch` (a computed ref).
 *
 * @returns The current header values.
 */
const sentHeaders = (): Record<string, string> => {
  const options = useFetchMock.mock.calls[0]?.[1] as { headers: { value: Record<string, string> } }
  return options.headers.value
}

describe('useApi', () => {
  it('stamps X-Language from the active locale and never the retired x-locale', () => {
    locale.value = 'ru'

    useApi('/api/portal/client/me')

    expect(sentHeaders()['X-Language']).toBe('ru')
    expect(sentHeaders()['x-locale']).toBeUndefined()
  })

  it('follows a locale change without rebuilding the call', () => {
    useApi('/api/portal/client/me')
    expect(sentHeaders()['X-Language']).toBe('en')

    locale.value = 'hy'

    expect(sentHeaders()['X-Language']).toBe('hy')
  })

  it('sends the default language when the locale is not one the backend ships', () => {
    locale.value = 'fr'

    useApi('/api/portal/client/me')

    expect(sentHeaders()['X-Language']).toBe('en')
  })

  it('keeps the caller headers and keys the cache by locale', () => {
    locale.value = 'hy'

    useApi('/api/portal/public/slides', { headers: { 'X-Extra': '1' } })

    expect(sentHeaders()).toMatchObject({ 'X-Language': 'hy', 'X-Extra': '1' })
    expect((useFetchMock.mock.calls[0]?.[1] as { key: string }).key).toContain('api:hy:')
  })
})

describe('apiFetch', () => {
  /**
   * Points `$fetch` at the stub and fakes the Nuxt app for the given locale.
   *
   * @param language - Locale the fake i18n plugin reports.
   */
  const arrange = async (language: string) => {
    stub = await startStubServer()
    vi.stubGlobal('$fetch', ofetch.create({ baseURL: stub.baseUrl }))
    vi.stubGlobal('useNuxtApp', () => ({
      $i18n: { locale: { value: language } },
      $handleAuthExpired: handleAuthExpired,
      runWithContext: (callback: () => Promise<void>) => callback(),
    }))
  }

  it('sends X-Language on the wire', async () => {
    await arrange('ru')

    await apiFetch('/ping')

    expect(stub?.requests[0]?.headers['x-language']).toBe('ru')
    expect(stub?.requests[0]?.headers['x-locale']).toBeUndefined()
  })

  it('follows the locale at call time', async () => {
    await arrange('en')
    await apiFetch('/first')
    vi.stubGlobal('useNuxtApp', () => ({ $i18n: { locale: { value: 'hy' } }, runWithContext: (callback: () => Promise<void>) => callback() }))

    await apiFetch('/second')

    expect(stub?.requests.map((request) => request.headers['x-language'])).toEqual(['en', 'hy'])
  })

  it('calls the global $fetch as it is at call time, so a plugin that replaces it takes effect', async () => {
    await arrange('en')
    // What `plugins/csrf-header.ts` does after the module has loaded: swap the global for a
    // wrapper that stamps a header. A fetcher captured at load time would never see it.
    vi.stubGlobal('$fetch', ofetch.create({
      baseURL: stub?.baseUrl,
      onRequest({ options }) {
        options.headers.set('X-Requested-With', 'XMLHttpRequest')
      },
    }))

    await apiFetch('/ping')

    expect(stub?.requests[0]?.headers['x-requested-with']).toBe('XMLHttpRequest')
  })

  it('answers in the default language when there is no Nuxt instance', async () => {
    stub = await startStubServer()
    vi.stubGlobal('$fetch', ofetch.create({ baseURL: stub.baseUrl }))
    vi.stubGlobal('useNuxtApp', () => {
      throw new Error('no nuxt app')
    })

    await apiFetch('/ping')

    expect(stub.requests[0]?.headers['x-language']).toBe('en')
  })

  it('lets the caller override a header and keeps the language', async () => {
    await arrange('hy')

    await apiFetch('/ping', { headers: { 'X-Requested-With': 'XMLHttpRequest' } })

    expect(stub?.requests[0]?.headers).toMatchObject({ 'x-language': 'hy', 'x-requested-with': 'XMLHttpRequest' })
  })

  it('hands a 401 to the auth guard and rethrows it', async () => {
    await arrange('en')
    stub?.respondWith(() => ({ status: 401, body: { type: 'UNAUTHORIZED' } }))

    await expect(apiFetch('/ping')).rejects.toMatchObject({ statusCode: 401 })

    expect(handleAuthExpired).toHaveBeenCalledTimes(1)
  })

  it('rethrows any other failure without touching the auth guard', async () => {
    await arrange('en')
    stub?.respondWith(() => ({ status: 400, body: { type: 'VALIDATION_ERROR', detail: 'Bad.' } }))

    await expect(apiFetch('/ping')).rejects.toMatchObject({ statusCode: 400, data: { detail: 'Bad.' } })

    expect(handleAuthExpired).not.toHaveBeenCalled()
  })
})

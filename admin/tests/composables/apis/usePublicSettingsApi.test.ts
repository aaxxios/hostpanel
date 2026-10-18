/**
 * Tests for the public-settings API composable: the anonymous branding read goes to the public
 * endpoint and a refusal surfaces as an `ApiError` carrying the API's body.
 */

import { afterEach, describe, expect, it, vi } from 'vitest'
import { usePublicSettingsApi } from '@/composables/apis/usePublicSettingsApi'
import { ApiError } from '@/composables/useApi'
import { callOf, json, problem, stubFetch } from '../../helpers/fetch'

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('usePublicSettingsApi', () => {
  it('fetchPublicSettings reads the public settings endpoint', async () => {
    const mock = stubFetch(json([{ key: 'portal.logo', value: '/l.png' }]))

    expect(await usePublicSettingsApi().fetchPublicSettings()).toEqual([{ key: 'portal.logo', value: '/l.png' }])
    expect(callOf(mock).url).toBe('/api/settings/public')
    expect(callOf(mock).method).toBe('GET')
  })

  it('fetchPublicSettings rejects with the API body on a refusal', async () => {
    stubFetch(problem('Unavailable', 503))

    await expect(usePublicSettingsApi().fetchPublicSettings()).rejects.toBeInstanceOf(ApiError)
  })
})

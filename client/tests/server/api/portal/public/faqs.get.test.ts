/**
 * Tests for `GET /api/portal/public/faqs`: it reads the public FAQ feed, forwards the reader's
 * language, passes the API's topic name (the client's `FaqCategory` value) through and passes a refusal
 * through.
 *
 * @module tests/server/api/portal/public/faqs.get.test
 */

import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { makeEvent, refusal, startBackend } from '../../../../helpers/bffHarness'
import type { RouteModule } from '../../../../helpers/bffHarness'
import type { StubServer } from '../../../../helpers/stubServer'
import { faqFixture } from '../../../../helpers/storefrontFixtures'
import { FaqCategory } from '~/types/faqcategory'

/** The stub playing the C# API. */
let backend: StubServer

/** The route under test. */
let route: RouteModule

beforeEach(async () => {
  backend = await startBackend()
  route = await import('~/server/api/portal/public/faqs.get')
})

afterEach(async () => {
  await backend.close()
})

describe('GET /api/portal/public/faqs', () => {
  it('requests the public feed, forwarding the reader language', async () => {
    backend.respondWith(() => ({ body: [{ ...faqFixture(1), category: 'General' }] }))

    await route.default(makeEvent({ headers: { 'x-language': 'ru' } }))

    expect(backend.requests[0]?.url).toBe('/api/public/faqs')
    expect(backend.requests[0]?.headers['x-language']).toBe('ru')
  })

  it('passes the topic name the API serializes through unchanged', async () => {
    backend.respondWith(() => ({
      body: [
        { ...faqFixture(1), category: 'General' },
        { ...faqFixture(2), category: 'Products' },
      ],
    }))

    const result = await route.default(makeEvent()) as Array<{ category: FaqCategory }>

    expect(result.map(row => row.category)).toEqual([FaqCategory.General, FaqCategory.Products])
  })

  it('passes an empty feed through, which hides the section', async () => {
    backend.respondWith(() => ({ body: [] }))

    expect(await route.default(makeEvent())).toEqual([])
  })

  it('passes the backend refusal through with its code and sentence', async () => {
    backend.respondWith(() => ({ status: 503, body: { type: 'UNAVAILABLE', detail: 'Try later.' } }))

    const error = await refusal(() => route.default(makeEvent()))

    expect(error).toMatchObject({ statusCode: 503, statusMessage: 'Try later.', data: { code: 'UNAVAILABLE' } })
  })
})

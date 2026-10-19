/**
 * Tests for `composables/apis/useStorefrontApi.ts`.
 *
 * Each endpoint function is called for real through `apiFetch` against a stub server, and the
 * test asserts on the path and verb that arrived and on how the reply was typed.
 *
 * @module tests/composables/apis/useStorefrontApi.test
 */

import { afterEach, describe, expect, it } from 'vitest'
import { expectLastRequest, startTransport } from '../../helpers/stubTransport'
import type { ExpectedRequest } from '../../helpers/stubTransport'
import type { StubServer } from '../../helpers/stubServer'
import { faqFixture, partnerFixture, projectFixture, testimonialFixture } from '../../helpers/storefrontFixtures'
import { useStorefrontApi } from '~/composables/apis/useStorefrontApi'

/** The stub started by the current test. */
let stub: StubServer | undefined

afterEach(async () => {
  await stub?.close()
  stub = undefined
})

/** The api surface under test. */
const api = useStorefrontApi()

/** Every endpoint function with the request it must send. */
const rows: ExpectedRequest[] = [
  { name: 'fetchTestimonials reads the testimonials', call: () => api.fetchTestimonials(), method: 'GET', url: '/api/portal/public/testimonials' },
  { name: 'fetchPartners reads the partners', call: () => api.fetchPartners(), method: 'GET', url: '/api/portal/public/partners' },
  { name: 'fetchProjects reads the projects', call: () => api.fetchProjects(), method: 'GET', url: '/api/portal/public/projects' },
  { name: 'fetchFaqs reads the questions', call: () => api.fetchFaqs(), method: 'GET', url: '/api/portal/public/faqs' },
]

describe('useStorefrontApi', () => {
  it.each(rows)('$name', async (row) => {
    stub = await startTransport()

    await row.call()

    expectLastRequest(stub, row)
  })

  it('returns the lists the endpoints promise', async () => {
    stub = await startTransport((request) => {
      const bodies: Record<string, unknown> = {
        '/api/portal/public/testimonials': [testimonialFixture(1)],
        '/api/portal/public/partners': [partnerFixture(1)],
        '/api/portal/public/projects': [projectFixture(1)],
        '/api/portal/public/faqs': [faqFixture(1)],
      }
      return { body: bodies[request.url] }
    })

    expect((await api.fetchTestimonials())[0]?.quote).toBe('Example quote 1.')
    expect((await api.fetchPartners())[0]?.name).toBe('Example Co 1')
    expect((await api.fetchProjects())[0]?.slug).toBe('example-project-1')
    expect((await api.fetchFaqs())[0]?.question).toBe('Example question 1?')
  })
})

/**
 * Tests for `tests/helpers/runtimeStubs.ts`: the one stand-in for the route auto-imports.
 *
 * @module tests/helpers/runtimeStubs.test
 */

import { describe, expect, it } from 'vitest'
import { stubLocalePath, stubRoute } from '~/tests/helpers/runtimeStubs'

describe('stubRoute', () => {
  it('answers with the given parameters, query and path', () => {
    stubRoute({ params: { id: '7' }, query: { tab: 'x' }, fullPath: '/a?tab=x' })

    const route = useRoute()

    expect(route.params).toEqual({ id: '7' })
    expect(route.query).toEqual({ tab: 'x' })
    expect(route.fullPath).toBe('/a?tab=x')
  })

  it('defaults to an empty route at the root', () => {
    stubRoute()

    expect(useRoute().fullPath).toBe('/')
  })
})

describe('stubLocalePath', () => {
  it('keeps the path without a prefix and adds one when asked', () => {
    stubLocalePath()
    expect(useLocalePath()('/hosting')).toBe('/hosting')

    stubLocalePath('/en')
    expect(useLocalePath()('/hosting')).toBe('/en/hosting')
  })
})

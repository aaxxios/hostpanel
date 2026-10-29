/**
 * Tests for route prefetching: lazy loaders of every matched record are started once, resolved
 * components are left alone, and a failed download is retried by the next call.
 */
import { describe, expect, it, vi } from 'vitest'
import type { RouteLocationNormalized } from 'vue-router'
import { withRetry } from '@/lib/lazyRoute'
import { prefetchRoute } from '@/lib/prefetchRoute'

/**
 * Builds the slice of a resolved route that prefetching reads.
 *
 * @param components - One `components` map per matched record.
 * @returns A route whose `matched` holds those records.
 */
const routeOf = (...components: Array<Record<string, unknown>>): Pick<RouteLocationNormalized, 'matched'> =>
  ({ matched: components.map((map) => ({ components: map })) }) as unknown as Pick<RouteLocationNormalized, 'matched'>

describe('prefetchRoute', () => {
  it('starts the loader of every matched record, layout first', () => {
    const layout = vi.fn(() => Promise.resolve({}))
    const view = vi.fn(() => Promise.resolve({}))

    prefetchRoute(routeOf({ default: layout }, { default: view }))

    expect(layout).toHaveBeenCalledTimes(1)
    expect(view).toHaveBeenCalledTimes(1)
    expect(layout.mock.invocationCallOrder[0]).toBeLessThan(view.mock.invocationCallOrder[0] as number)
  })

  it('starts the import behind a retrying route loader', () => {
    const loader = vi.fn(() => Promise.resolve({}))

    prefetchRoute(routeOf({ default: withRetry(loader) }))

    expect(loader).toHaveBeenCalledTimes(1)
  })

  it('requests a loader only once', () => {
    const view = vi.fn(() => Promise.resolve({}))

    prefetchRoute(routeOf({ default: view }))
    prefetchRoute(routeOf({ default: view }))

    expect(view).toHaveBeenCalledTimes(1)
  })

  it('leaves already-resolved components alone', () => {
    const plain = { render: () => null }
    const withName = Object.assign(() => null, { displayName: 'Named' })
    const withProps = Object.assign(() => null, { props: {} })
    const compiled = Object.assign(() => null, { __vccOpts: {} })

    expect(() => prefetchRoute(routeOf({ default: plain }, { default: withName }, { default: withProps }, { default: compiled }))).not.toThrow()
  })

  it('retries a loader whose download failed', async () => {
    const view = vi.fn(() => Promise.reject(new Error('offline')))

    prefetchRoute(routeOf({ default: view }))
    await Promise.resolve()
    await Promise.resolve()
    prefetchRoute(routeOf({ default: view }))

    expect(view).toHaveBeenCalledTimes(2)
  })

  it('does nothing for a route that matched nothing', () => {
    expect(() => prefetchRoute(routeOf())).not.toThrow()
  })
})

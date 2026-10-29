/**
 * Route table snapshot: every route's path, name, meta, redirect kind and the file basename of its
 * lazy component must equal the table recorded before the layout move (`routes.snapshot.json`).
 * A moved view keeps its file name, so the snapshot stays identical across the move. Every routed
 * component is a plain lazy loader, the only lazy form vue-router supports.
 */
import { describe, expect, it } from 'vitest'
import type { RouteRecordRaw } from 'vue-router'
import router from '@/router'
import { isLazyLoader, originalLoader } from '@/lib/lazyRoute'
import expected from './routes.snapshot.json'

/** One snapshotted route. */
interface RouteSnapshot {
  /** Route path as declared. */
  path: string
  /** Route name, when declared. */
  name?: string
  /** Route meta, when declared. */
  meta?: Record<string, unknown>
  /** Redirect target, or the word `function` for a redirect function. */
  redirect?: string
  /** File basename of the lazy component. */
  component?: string
  /** Nested routes. */
  children?: RouteSnapshot[]
}

/**
 * Extracts the `.vue` file basename from a lazy loader's source text.
 *
 * @param loader - The dynamic `import()` function of a route.
 * @returns The basename, or undefined when the source names no `.vue` file.
 */
const basenameOf = (loader: () => unknown): string | undefined => {
  const match = /([A-Za-z0-9_]+\.vue)/.exec(loader.toString())
  return match ? match[1] : undefined
}

/**
 * Converts raw route records into the snapshot shape.
 *
 * @param routes - Raw route records.
 * @returns Snapshot entries with the same nesting.
 */
const describeRoutes = (routes: readonly RouteRecordRaw[]): RouteSnapshot[] => {
  return routes.map((route) => {
    const entry: RouteSnapshot = { path: route.path }
    if (route.name !== undefined) {
      entry.name = String(route.name)
    }
    if (route.meta) {
      entry.meta = route.meta as Record<string, unknown>
    }
    if (route.redirect !== undefined) {
      entry.redirect = typeof route.redirect === 'function' ? 'function' : String(route.redirect)
    }
    if ('component' in route && isLazyLoader(route.component)) {
      // Every view and layout is a retrying loader; the file name is in the import it wraps.
      entry.component = basenameOf(originalLoader(route.component))
    }
    if (route.children) {
      entry.children = describeRoutes(route.children)
    }
    return entry
  })
}

/**
 * Lists every component of a route table, nested routes included.
 *
 * @param routes - Raw route records.
 * @returns The `component` of each record that has one.
 */
const componentsOf = (routes: readonly RouteRecordRaw[]): unknown[] =>
  routes.flatMap((route) => [
    ...('component' in route && route.component !== undefined ? [route.component] : []),
    ...componentsOf(route.children ?? []),
  ])

describe('route table', () => {
  it('matches the snapshot recorded before the layout move', () => {
    expect(describeRoutes(router.options.routes)).toEqual(expected)
  })

  it('gives every routed component as a plain lazy loader, never as an async component', () => {
    const components = componentsOf(router.options.routes)

    expect(components.length).toBeGreaterThan(0)
    // vue-router warns about (and does not await) a route component made with defineAsyncComponent.
    expect(components.filter((component) => !isLazyLoader(component))).toEqual([])
  })
})

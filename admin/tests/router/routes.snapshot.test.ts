/**
 * Route table snapshot: every route's path, name, meta, redirect kind and the file basename of its
 * lazy component must equal the table recorded before the layout move (`routes.snapshot.json`).
 * A moved view keeps its file name, so the snapshot stays identical across the move.
 */
import { describe, expect, it } from 'vitest'
import type { RouteRecordRaw } from 'vue-router'
import router from '@/router'
import { originalLoader, viewLoader } from '@/lib/lazyView'
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
 * @param loader - The `() => import('…')` function of a route.
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
    if ('component' in route && route.component) {
      // A layout is a plain loader; a view is an async component whose loader the registry keeps.
      const loader = typeof route.component === 'function' ? originalLoader(route.component as () => Promise<unknown>) : viewLoader(route.component)
      if (loader !== undefined) {
        entry.component = basenameOf(loader as () => unknown)
      }
    }
    if (route.children) {
      entry.children = describeRoutes(route.children)
    }
    return entry
  })
}

describe('route table', () => {
  it('matches the snapshot recorded before the layout move', () => {
    expect(describeRoutes(router.options.routes)).toEqual(expected)
  })
})

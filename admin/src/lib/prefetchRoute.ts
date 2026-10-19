/**
 * Route prefetching: starts downloading the lazy chunks of a screen before the operator gets there.
 *
 * Every screen is its own chunk, and vue-router will not change the URL until the chunk of the
 * target has arrived. On a slow link that wait is the whole "the menu thinks before it opens" delay,
 * so the chunk is requested as soon as the pointer is over the link (or, on the first page load,
 * in parallel with the session check) and the click then finds it already in the module cache.
 */
import type { Component } from 'vue'
import type { RouteLocationNormalized, RouteRecordNormalized } from 'vue-router'
import { viewLoader } from '@/lib/lazyView'

/** A lazy component loader as vue-router stores it in a route record. */
type LazyLoader = () => Promise<unknown>

/** Loaders whose download has started or finished; a loader is requested at most once unless it failed. */
const requested = new WeakSet<LazyLoader>()

/**
 * Tells a lazy loader from an already-resolved component.
 *
 * vue-router keeps either in `record.components`; a component is an object, or a function carrying
 * one of the markers below, and anything else callable is a `() => import(...)` loader.
 *
 * @param candidate - A value from `record.components`.
 * @returns True when the value is a loader.
 */
const isLazyLoader = (candidate: unknown): candidate is LazyLoader => {
  if (typeof candidate !== 'function') {
    return false
  }
  return !('displayName' in candidate) && !('props' in candidate) && !('__vccOpts' in candidate)
}

/**
 * Finds what to call to download a component of a route record.
 *
 * @param component - A value from `record.components`.
 * @returns The loader of an async screen or a plain lazy layout, or undefined for a component already in memory.
 */
const loaderOf = (component: unknown): LazyLoader | undefined => {
  if (isLazyLoader(component)) {
    return component
  }
  return viewLoader(component as Component)
}

/**
 * Starts the download of one loader, once.
 *
 * A failed download is forgotten, so the click that follows retries it instead of inheriting the failure.
 *
 * @param loader - The `() => import(...)` function of a route record.
 */
const request = (loader: LazyLoader): void => {
  if (requested.has(loader)) {
    return
  }
  requested.add(loader)
  loader().catch(() => {
    requested.delete(loader)
  })
}

/**
 * Starts downloading every lazy chunk the given route needs: its layouts and its view.
 *
 * Fire and forget: nothing is awaited and nothing throws, so it is safe to call from a hover handler
 * or a navigation guard without delaying either.
 *
 * @param route - The resolved target, whose `matched` records name the components.
 */
export const prefetchRoute = (route: Pick<RouteLocationNormalized, 'matched'>): void => {
  route.matched.forEach((record: RouteRecordNormalized) => {
    Object.values(record.components ?? {}).forEach((component) => {
      const loader = loaderOf(component)
      if (loader !== undefined) {
        request(loader)
      }
    })
  })
}

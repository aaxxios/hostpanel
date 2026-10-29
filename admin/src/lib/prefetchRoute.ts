/**
 * Route prefetching: starts downloading the lazy chunks of a screen before the operator gets there.
 *
 * Every screen is its own chunk, and vue-router will not change the URL until the chunk of the
 * target has arrived. On a slow link that wait is the whole "the menu thinks before it opens" delay,
 * so the chunk is requested as soon as the pointer is over the link (or, on the first page load,
 * in parallel with the session check) and the click then finds it already in the module cache.
 */
import type { RouteLocationNormalized } from 'vue-router'
import { lazyLoadersOf } from '@/lib/lazyRoute'
import type { RouteLoader } from '@/lib/lazyRoute'

/** Loaders whose download has started or finished; a loader is requested at most once unless it failed. */
const requested = new WeakSet<RouteLoader>()

/**
 * Starts the download of one loader, once.
 *
 * A failed download is forgotten, so the click that follows retries it instead of inheriting the failure.
 *
 * @param loader - The dynamic `import()` function of a route record.
 */
const request = (loader: RouteLoader): void => {
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
  lazyLoadersOf(route).forEach(request)
}

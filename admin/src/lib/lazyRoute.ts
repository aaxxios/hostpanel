/**
 * Lazy route loaders that survive a dropped download, and the helpers that find them in a route.
 *
 * Every routed screen and layout is its own chunk, handed to vue-router as a plain `() => import()`
 * loader, which is the only lazy form the router supports. The loader is wrapped by
 * {@link withRetry} so a failed download is repeated before the navigation gives up. The plain
 * import behind a wrapper stays reachable through {@link originalLoader}, and
 * {@link lazyLoadersOf} lists the loaders a route still has to call, which is what prefetching and
 * the pending-navigation state read.
 */
import type { RouteLocationNormalized } from 'vue-router'

/** A dynamic `import()` function as vue-router keeps it in a route record. */
export type RouteLoader = () => Promise<unknown>

/** How many times a failed download is repeated before the navigation fails. */
export const LOAD_RETRIES = 2

/** Wait before the first repeat, in milliseconds; each further repeat waits one step longer. */
export const RETRY_STEP_MS = 400

/** The plain import behind each retrying wrapper made by {@link withRetry}. */
const originals = new WeakMap<object, RouteLoader>()

/**
 * Waits for a number of milliseconds.
 *
 * @param ms - How long to wait.
 * @returns A promise that resolves after that time.
 */
const pause = (ms: number): Promise<void> => new Promise((resolve) => setTimeout(resolve, ms))

/**
 * Makes a loader repeat itself when the download fails.
 *
 * A gateway timeout or a dropped connection on one small chunk is common on a long path, and
 * without a repeat it leaves the click with no effect at all.
 *
 * @param loader - The dynamic `import()` function.
 * @returns A loader that tries up to {@link LOAD_RETRIES} more times before it gives up with the last error.
 */
export const withRetry = <T>(loader: () => Promise<T>): (() => Promise<T>) => {
  /**
   * Runs the loader, repeating it after a growing pause while it fails.
   *
   * @returns What the loader resolves to.
   * @throws The last error once the repeats are used up.
   */
  const retrying = async (): Promise<T> => {
    let attempt = 0
    for (;;) {
      try {
        return await loader()
      } catch (error) {
        if (attempt >= LOAD_RETRIES) {
          throw error
        }
        attempt += 1
        await pause(RETRY_STEP_MS * attempt)
      }
    }
  }
  originals.set(retrying, loader)
  return retrying
}

/**
 * Finds the plain import behind a loader made by {@link withRetry}.
 *
 * @param loader - A loader that may be a retrying wrapper.
 * @returns The import it wraps, or the loader itself when it is not a wrapper.
 */
export const originalLoader = (loader: RouteLoader): RouteLoader => originals.get(loader) ?? loader

/**
 * Tells a lazy loader from an already-resolved component.
 *
 * vue-router keeps either in `record.components`, and swaps a loader for the component it
 * delivered once the download is done. It treats a function carrying one of the markers below as a
 * component, and anything else callable as a dynamic `import()` loader; this mirrors that test.
 *
 * @param candidate - A value from `record.components`.
 * @returns True when the value is a loader the router still has to call.
 */
export const isLazyLoader = (candidate: unknown): candidate is RouteLoader => {
  if (typeof candidate !== 'function') {
    return false
  }
  return !('displayName' in candidate) && !('props' in candidate) && !('__vccOpts' in candidate)
}

/**
 * Lists the loaders a route still has to call before it can render, outermost layout first.
 *
 * @param route - A resolved route, whose `matched` records name the components.
 * @returns The loaders of its layouts and its view that have not delivered yet; empty when everything is in memory.
 */
export const lazyLoadersOf = (route: Pick<RouteLocationNormalized, 'matched'>): RouteLoader[] =>
  route.matched.flatMap((record) => Object.values(record.components ?? {}).filter(isLazyLoader))

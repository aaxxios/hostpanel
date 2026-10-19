/**
 * Lazy screens that never hold the navigation back.
 *
 * vue-router keeps the old screen up, and the address bar unchanged, until the chunk of the next
 * screen has arrived. Over a slow link that reads as a menu that "thinks". A screen made with
 * {@link lazyView} is an async component instead: the router switches to it immediately, the sidebar
 * highlights it, and a skeleton stands in while the chunk downloads. The loader stays reachable
 * through {@link viewLoader}, so hovering a link can still start the download early.
 */
import { defineAsyncComponent } from 'vue'
import type { Component } from 'vue'
import ViewLoadError from '@/components/layout/ViewLoadError.vue'
import ViewLoading from '@/components/layout/ViewLoading.vue'

/** What a dynamic `import()` of a screen resolves to: the component, or a module whose default export is. */
type ScreenModule = Component | { default: Component }

/** A function that downloads one screen. */
export type ScreenLoader = () => Promise<ScreenModule>

/** How many times a failed download is repeated before the screen shows its error. */
export const LOAD_RETRIES = 2

/** Wait before the first repeat, in milliseconds; each further repeat waits one step longer. */
export const RETRY_STEP_MS = 400

/** The import behind each async screen, for prefetching. */
const loaders = new WeakMap<Component, ScreenLoader>()

/** The plain import behind each retrying wrapper made by {@link withRetry}. */
const originals = new WeakMap<object, () => Promise<unknown>>()

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
 * @param loader - The `() => import(...)` function.
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
export const originalLoader = (loader: () => Promise<unknown>): () => Promise<unknown> => originals.get(loader) ?? loader

/**
 * Wraps a screen loader as an async component with a skeleton and an error state.
 *
 * @param loader - The `() => import(...)` function of the screen.
 * @returns A component to put in a route record.
 */
export const lazyView = (loader: ScreenLoader): Component => {
  const retrying = withRetry(loader)
  const component = defineAsyncComponent({
    loader: retrying,
    loadingComponent: ViewLoading,
    errorComponent: ViewLoadError,
    // Straight away: the point is that the click is acknowledged at once.
    delay: 0,
    // No `timeout`: the chunk may legitimately take a while on a slow link, and the repeats above already
    // bound the wait on failure. (`Infinity` would not mean "never": setTimeout coerces it to 0.)
  })
  loaders.set(component, loader)
  return component
}

/**
 * Finds the loader behind an async screen made by {@link lazyView}.
 *
 * @param component - A component from a route record.
 * @returns Its loader, or undefined for any other component.
 */
export const viewLoader = (component: Component): ScreenLoader | undefined => loaders.get(component)

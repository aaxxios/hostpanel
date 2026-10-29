/**
 * The navigation store: which navigation is still downloading its screens and which one could not.
 *
 * vue-router keeps the old screen and the old address until every chunk of the target has arrived.
 * This store is what lets the shell acknowledge a click before that: the router guards in
 * `middleware/navigation.ts` record the navigation here, and `RoutedView` draws a skeleton or the
 * load error in place of the screen that is about to change.
 */
import { defineStore } from 'pinia'
import { shallowRef } from 'vue'
import type { RouteLocationNormalized } from 'vue-router'

/**
 * Pinia store holding the state of the navigation in flight.
 *
 * Locations are held in shallow refs and compared by identity: vue-router hands the same object to
 * every guard and hook of one navigation, which is how a late hook of an older navigation is told
 * apart from the current one. A deep ref would wrap the object in a proxy and break that comparison.
 */
export const useNavigationStore = defineStore('navigation', () => {
  /** The target whose screens are still downloading; null when nothing is pending. */
  const pending = shallowRef<RouteLocationNormalized | null>(null)

  /** The target whose screens could not be downloaded, even after the repeats; null when the last navigation did not fail. */
  const failed = shallowRef<RouteLocationNormalized | null>(null)

  /**
   * The most recent navigation that passed the guards.
   *
   * Not state anything renders: it only decides which hook may still change {@link pending} and {@link failed}.
   */
  let latest: RouteLocationNormalized | null = null

  /**
   * Records a navigation that passed every guard and is about to load its screens.
   *
   * @param to - The target, as vue-router passes it to the guards.
   * @param downloads - Whether any of its layouts or its view still has to be downloaded.
   */
  const admit = (to: RouteLocationNormalized, downloads: boolean): void => {
    latest = to
    failed.value = null
    pending.value = downloads ? to : null
  }

  /**
   * Ends a navigation, whether it arrived, was cancelled by a newer one or was refused.
   *
   * @param to - The target the navigation was heading for.
   */
  const settle = (to: RouteLocationNormalized): void => {
    if (latest !== to) {
      return
    }
    pending.value = null
  }

  /**
   * Records a navigation that ended in an error, typically a screen whose chunk never arrived.
   *
   * @param to - The target the navigation was heading for.
   */
  const fail = (to: RouteLocationNormalized): void => {
    if (latest !== to) {
      return
    }
    pending.value = null
    failed.value = to
  }

  return { pending, failed, admit, settle, fail }
})

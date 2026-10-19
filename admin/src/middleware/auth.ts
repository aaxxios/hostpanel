/** Navigation guard for the admin router: loads the session state and redirects by route meta. */
import type { RouteLocationNormalized } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { AuthMode } from '@/types/auth'
import { pinia } from '@/pinia'
import { AppRoute } from '@/lib/routes'

/**
 * Auth middleware — decides, per navigation, whether a route may be entered.
 *
 * Everything it decides on comes from the server. There is no longer a
 * `localStorage` "setup complete" flag: it was written by whichever browser finished
 * setup and read by nothing, so a second machine, a cleared profile or a private window
 * all disagreed with it, and the API answers the same question authoritatively at
 * `/auth/setup-required`.
 *
 * @param to - Target route being navigated to.
 * @returns The redirect path, or true to allow the navigation.
 */
export const authMiddleware = async (
  to: RouteLocationNormalized,
): Promise<string | boolean> => {
  const auth = useAuthStore(pinia)

  // Which mode this deployment runs, whether anyone holds Admin yet, and whether a session exists.
  // The three are independent questions only the server can answer (the session credential is an
  // httpOnly cookie the page cannot read), so they go out together: asking them one after the other
  // cost a full round trip each before the first screen could open. All are asked once per page
  // load rather than per navigation; afterwards the guard reads the store and makes no request.
  await Promise.all([
    auth.mode === null ? auth.loadMode() : Promise.resolve(),
    auth.user === null ? auth.fetchMe() : Promise.resolve(),
  ])

  // The first-run screen. It creates the very first local account, so it is meaningful
  // only while nobody holds Admin and only where this deployment owns its own accounts.
  // On an SSO deployment there is nothing for it to create — the accounts live in the
  // sign-on service — and reaching it would call endpoints that answer 404 there.
  if (to.meta.setup === true) {
    if (!auth.setupRequired || auth.mode !== AuthMode.Local) {
      return AppRoute.Login
    }
    return true
  }

  // Authenticated user trying to access /login → redirect to dashboard
  if (to.meta.public && auth.isAuthenticated) {
    return AppRoute.Dashboard
  }

  // Protected route without valid session → redirect to login
  if (to.meta.requiresAuth && !auth.isAuthenticated) {
    return AppRoute.Login
  }

  return true
}

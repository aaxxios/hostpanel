/**
 * The shared Playwright `test` object for the hostpanel client's e2e specs.
 *
 * Every spec imports `test` and `expect` from here instead of `@playwright/test`, so the suite
 * never depends on the internet: the storefront embeds third-party widgets (a map, analytics,
 * web fonts, live chat) whose requests hang or fail inside a container and keep the page from
 * ever reaching `networkidle`. An automatic fixture aborts every request whose origin is not the
 * app server or the stub backend, in every context the test opens.
 *
 * @module e2e/support/test
 */
import { expect, test as base } from '@playwright/test'

/** Port the client listens on; mirrors `playwright.config.ts`. */
const APP_PORT = String(Number(process.env.E2E_PORT || 3100))

/** Port the stub backend listens on; mirrors `playwright.config.ts`. */
const STUB_PORT = String(Number(process.env.E2E_STUB_PORT || 5999))

/** Hosts that mean "this machine". */
const LOCAL_HOSTS: ReadonlySet<string> = new Set(['127.0.0.1', 'localhost'])

/** URL schemes that never leave the browser and so are never blocked. */
const OFFLINE_SCHEMES: ReadonlySet<string> = new Set(['data:', 'blob:', 'about:'])

/**
 * Decides whether a request may reach the network: only the app server and the stub backend.
 *
 * @param rawUrl The absolute URL the browser is about to request.
 * @returns `true` when the request is local to the test run.
 */
export const isLocalRequest = (rawUrl: string): boolean => {
  const url = new URL(rawUrl)
  if (OFFLINE_SCHEMES.has(url.protocol)) {
    return true
  }
  return LOCAL_HOSTS.has(url.hostname) && (url.port === APP_PORT || url.port === STUB_PORT)
}

/**
 * `test` with an automatic, offline-only network guard; use it in place of Playwright's own.
 */
export const test = base.extend<{ offlineOnly: undefined }>({
  offlineOnly: [
    async ({ context }, use) => {
      await context.route(
        (url) => !isLocalRequest(url.href),
        (route) => route.abort('blockedbyclient'),
      )
      await use(undefined)
    },
    { auto: true },
  ],
})

export { expect }

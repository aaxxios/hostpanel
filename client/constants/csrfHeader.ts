/**
 * The custom header that proves a state-changing request was made by this app's own code.
 *
 * Both hops need it: the browser calling the Nuxt server, which `server/middleware/csrf.ts`
 * checks, and the Nuxt server calling the C# API, where an SSO build refuses an anonymous
 * state-changing request without it (the domain availability check answered 403 for every
 * visitor because that second hop did not send it).
 *
 * @module constants/csrfHeader
 */

/** Name of the header. */
export const CSRF_HEADER = 'X-Requested-With'

/** The value the guards accept. */
export const CSRF_HEADER_VALUE = 'XMLHttpRequest'

/**
 * Runtime configuration of the admin SPA.
 *
 * Filled per deployment by `public/config.js` (the prod image regenerates that file from the
 * container environment at start), so one built image runs on any domain and base path.
 */
export interface AppRuntimeConfig {
  /** Public base path the SPA is served under, e.g. `/` or `/admin`. */
  basePath: string
}

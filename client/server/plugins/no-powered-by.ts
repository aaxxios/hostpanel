/**
 * Nitro plugin: never announce the framework.
 *
 * Nuxt's renderer stamps every page response with `x-powered-by: Nuxt`. The header tells a
 * scanner what to look up and nothing else, so it is removed on the way out. A plugin on the
 * `beforeResponse` hook, not a route rule: the renderer sets the header itself, after route rules
 * have been applied, so only a hook that runs once the response is assembled can take it off.
 *
 * @module server/plugins/no-powered-by
 */

import { removeResponseHeader } from 'h3'

/** The header the renderer sets and this plugin removes. */
export const POWERED_BY_HEADER = 'x-powered-by'

/** Registers the `beforeResponse` hook that removes the framework header. */
export default defineNitroPlugin((nitroApp) => {
  nitroApp.hooks.hook('beforeResponse', (event) => {
    removeResponseHeader(event, POWERED_BY_HEADER)
  })
})

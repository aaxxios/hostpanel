/**
 * Nitro plugin: build the session store at startup.
 *
 * The store refuses to be built when sessions are kept in Redis without a valid encryption key
 * (outside development). Building it here, rather than on the first sign-in, makes a
 * misconfigured deployment fail to start instead of failing for the first visitor.
 *
 * @module server/plugins/session-store
 */

import { useSessionStore } from '~/server/utils/session-store'

/** Builds the store once; a configuration error aborts the start. */
export default defineNitroPlugin(() => {
  useSessionStore()
})

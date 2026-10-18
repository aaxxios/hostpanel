/**
 * Demo-mode plugin.
 *
 * Loads the demo status once per request, before any layout renders, so the banner and the
 * sign-in page are server-rendered with the right answer. The store swallows failures, so a
 * backend that is down never stops the portal from rendering.
 *
 * @module plugins/demo-mode
 */
import { useDemoStore } from '~/stores/demo'

export default defineNuxtPlugin({
  name: 'demo-mode',
  async setup() {
    await useDemoStore().load()
  }
})

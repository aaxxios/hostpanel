/**
 * Loads the main portal's shared header widget — the app launcher and account chip the
 * client area shows (layouts/client.vue and the classic template's header carry its mounts).
 *
 * A plugin rather than nuxt.config's app.head on purpose: process.env inside nuxt.config
 * is read at BUILD time, so the src would bake whatever NUXT_PUBLIC_MAIN_URL the image was
 * built with. Read from runtimeConfig, the container's own environment decides.
 *
 * Not .client — runtimeConfig.public is available during SSR too, so the tag is rendered
 * into <head> with the document instead of appearing after hydration.
 *
 * Only an `sso` deployment loads it: the widget's account chip and switcher call the SSO-backed
 * `/api/portal/auth/sso/*` routes (a 500 under `local`, which has no SSO to reach) and frame the
 * portal's account page, so a standalone install has nothing for it to show.
 *
 * @module plugins/header-widget
 */

import { AuthMode } from '~/types/authmode'

export default defineNuxtPlugin(() => {
  const config = useRuntimeConfig()
  if (config.public.authMode !== AuthMode.Sso) {
    return
  }

  const mainUrl = config.public.mainUrl as string
  // The dev default is a host that exists only on a developer's machine; a stock install
  // with no portal must not request a script from it on every page.
  if (!mainUrl || /\.local(:\d+)?\/?$/.test(mainUrl)) {
    return
  }

  useHead({
    // `?v=` is a ONE-TIME cache bust shared with the other products. Do not bump it per
    // widget release: the portal used to serve /widget/header.js behind `expires 1y` +
    // `immutable`, and this token is what reaches browsers still holding that entry.
    script: [{ src: `${mainUrl}/widget/header.js?v=20260918`, defer: true }]
  })
})

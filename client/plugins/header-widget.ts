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

import { HEADER_WIDGET_CACHE_BUST, HEADER_WIDGET_PATH } from '~/constants/headerWidget'
import { isHeaderWidgetEnabled } from '~/utils/headerWidget'

export default defineNuxtPlugin(() => {
  const config = useRuntimeConfig()
  const mainUrl = config.public.mainUrl as string
  // The same rule the headers use to choose between this widget and their own account menu.
  if (!isHeaderWidgetEnabled(config.public.authMode as string | undefined, mainUrl)) {
    return
  }

  useHead({
    // The cache-bust token is explained where it is defined; it is deliberately not bumped per release.
    script: [{ src: `${mainUrl}${HEADER_WIDGET_PATH}?v=${HEADER_WIDGET_CACHE_BUST}`, defer: true }]
  })
})

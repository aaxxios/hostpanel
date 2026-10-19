/**
 * Tag-manager loader: loads Google Tag Manager once the operator has configured a
 * container **and** the visitor has accepted all cookies.
 *
 * Both halves used to be missing. The container id was a literal in `app.vue` — the
 * vendor's own, which every self-hosted install shipped and sent its visitors' data to —
 * and the loader was in the server-rendered HTML for everyone, consent or not, while the
 * cookie banner recorded an answer nothing read.
 *
 * Client-only by design: there is nothing to render on the server, and a tag that runs
 * before the visitor answered is the thing consent exists to prevent. The `<noscript>`
 * iframe fallback is gone with it; it cannot be gated on consent, and a visitor with
 * scripts off is not one who can be tracked anyway.
 *
 * @module plugins/tracking.client
 */

import { PortalSettingKey } from '~/constants/portalSettingKey'
import { CookieConsent } from '~/types/cookieconsent'

/** Address of the tag-manager loader script; the container id is appended as `id`. */
const GTM_SCRIPT_URL = 'https://www.googletagmanager.com/gtm.js'

/** The `window` as the tag manager extends it. */
interface WindowWithDataLayer {
  /** The queue the tag manager reads events from. */
  dataLayer?: unknown[]
}

/** Loads the tag manager once a container is configured and the visitor accepted all cookies. */
export default defineNuxtPlugin({
  name: 'tracking',
  dependsOn: ['portal-settings'],
  setup() {
    const { get } = usePortalSettings()
    const { consent } = useCookieConsent()

    /** The tag manager container id from the settings; empty when tracking is not configured. */
    const containerId = computed(() => get(PortalSettingKey.AnalyticsGtmId, 'portalGtmId'))

    /** True once the snippet has been injected. */
    let loaded = false

    /** Injects the standard GTM snippet for the configured container, once. */
    const load = (): void => {
      if (loaded || !containerId.value) {
        return
      }
      loaded = true

      const w = window as unknown as WindowWithDataLayer
      w.dataLayer = w.dataLayer ?? []
      w.dataLayer.push({ 'gtm.start': Date.now(), event: 'gtm.js' })

      const script = document.createElement('script')
      script.async = true
      script.src = `${GTM_SCRIPT_URL}?id=${encodeURIComponent(containerId.value)}`
      document.head.appendChild(script)
    }

    // Immediately for a returning visitor who already agreed, and again whenever the
    // banner records an answer. `essential` and "no answer yet" both mean no tag.
    watch(consent, (value) => {
      if (value === CookieConsent.All) {
        load()
      }
    }, { immediate: true })
  },
})

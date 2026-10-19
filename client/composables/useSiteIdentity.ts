/**
 * The name this storefront calls itself by, set by the operator.
 *
 * The operator's name used to be written into `nuxt.config.ts`, `useSchemaOrg.ts` and every
 * logo's `alt`; now it comes only from the `portal.site.name` setting or its runtime-config
 * fallback. An install that has set neither shows the neutral product name, never an operator's.
 */

import { PortalSettingKey } from '~/constants/portalSettingKey'

/** Product name shown when the operator has not set one. */
export const NEUTRAL_SITE_NAME = 'Hostpanel'

/**
 * Reads the site name and tagline the operator set.
 *
 * @returns Reactive `name` (never empty) and `tagline` (empty when unset).
 */
export const useSiteIdentity = () => {
  const { get } = usePortalSettings()

  /** The site name for titles, `og:site_name`, `alt` text and structured data. */
  const name = computed(() => get(PortalSettingKey.SiteName, 'portalSiteName') || NEUTRAL_SITE_NAME)

  /** A one-line description for the default meta description. Empty when unset. */
  const tagline = computed(() => get(PortalSettingKey.SiteTagline, 'portalSiteTagline'))

  return { name, tagline }
}

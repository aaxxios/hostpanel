/**
 * SEO composable for consistent meta tags across all pages
 * Handles canonical URLs, hreflang tags, Open Graph, and Twitter Cards
 *
 * Note: TypeScript may show errors for auto-imported composables (useI18n, useRoute, etc.)
 * during typecheck, but these are false positives - Nuxt auto-imports work at runtime.
 *
 * @module composables/useSeo
 */

import { AppLocale } from '~/types/applocale'
import { SeoPageType } from '~/types/seopagetype'
import { buildSeoLinks, localizedUrl, stripLocalePrefix } from '~/utils/seoLinks'
import { shareImageMeta } from '~/utils/shareImage'

/** What a page tells the SEO composable about itself. */
export interface SeoOptions {
  /** Page title. */
  title?: string
  /** Meta description. */
  description?: string
  /** Meta keywords. */
  keywords?: string
  /** Share image path, relative to the site root. */
  image?: string
  /** Open Graph object type. */
  type?: SeoPageType
  /** Path the canonical and alternate URLs are built from. */
  path?: string
}

/**
 * Sets the head tags a public page needs: title, description, canonical, hreflang and share cards.
 *
 * @param options What the page knows about itself.
 * @returns The canonical URL builder and the page's base values.
 */
export const useSeo = (options: SeoOptions) => {
  const { locale } = useI18n()
  const config = useRuntimeConfig()
  const route = useRoute()
  const { name: siteName } = useSiteIdentity()

  const baseUrl = config.public.baseUrl

  // Get current path or use provided path
  const currentPath = options.path || route.path

  // The page path in the default language, which every URL below is built from.
  const pagePath = stripLocalePrefix(currentPath)

  // Build canonical URL with locale
  const canonicalUrl = computed(() => localizedUrl(baseUrl, pagePath, locale.value))

  // Share image: the page's own, or the bundled card with its size and type. The alt text
  // names the page, or the site when the page has no title.
  const image = shareImageMeta(baseUrl, options.title || siteName.value, options.image)

  // OG Locale mapping
  const ogLocaleMap: Record<string, string> = {
    [AppLocale.En]: 'en_US',
    [AppLocale.Ru]: 'ru_RU',
    [AppLocale.Hy]: 'hy_AM'
  }

  // Set meta tags
  useSeoMeta({
    title: options.title,
    description: options.description,

    // Open Graph
    ogTitle: options.title,
    ogDescription: options.description,
    ...image,
    ogUrl: canonicalUrl.value,
    ogType: options.type || SeoPageType.Website,
    ogLocale: ogLocaleMap[locale.value] || 'en_US',

    // Twitter Card
    twitterCard: 'summary_large_image',
    twitterTitle: options.title,
    twitterDescription: options.description
  })

  // Set canonical and hreflang, and the keywords tag. `useSeoMeta` has no `keywords` member in the
  // current head library, so it is a plain meta entry here.
  useHead({
    link: buildSeoLinks(baseUrl, pagePath, locale.value),
    meta: options.keywords ? [{ name: 'keywords', content: options.keywords }] : []
  })

  return {
    canonicalUrl,
    baseUrl
  }
}

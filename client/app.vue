<template>
  <NuxtLayout>
    <NuxtPage />
  </NuxtLayout>
</template>

<script setup lang="ts">
/**
 * Root app component: global structured data, the favicon set and the site identity
 * metas. Third-party scripts live in plugins, driven by the operator's settings.
 */
import { PortalSettingKey } from '~/constants/portalSettingKey'
import type { HeadIconLink } from '~/types/headiconlink'
import { resolveLanguage } from '~/utils/transportLanguage'

/** Active UI locale; drives the `lang` attribute on `<html>`. */
const { locale } = useI18n()

/** Builders for the global structured data, and the function that injects them into the head. */
const { organizationSchema, localBusinessSchema, websiteSchema, injectSchema } = useSchemaOrg()

// Inject global schemas
injectSchema([
  organizationSchema(),
  localBusinessSchema(),
  websiteSchema()
])

// Live chat and the tag manager are loaded by plugins/live-chat.ts and
// plugins/tracking.client.ts from the operator's settings; nothing here names a
// server, a token or a container id.

/** Reads an operator setting by its key. */
const { get: getPortalSetting } = usePortalSettings()

/** Operator-uploaded favicon, admin-managed with an environment fallback. Empty keeps nuxt.config's static default. */
const faviconUrl = computed(() => getPortalSetting(PortalSettingKey.Favicon, 'portalFavicon'))

/**
 * Every `<link rel="icon">` the page emits, built-in defaults included.
 *
 * An uploaded favicon expands to the whole browser/iOS/Android set the API generated beside
 * it; a pasted URL stays a single link, because only that one file is known to exist; and
 * with nothing set at all this is where the built-in mark comes from — `nuxt.config` no
 * longer carries its own icon links, because two places emitting them meant an uploaded
 * favicon rendered alongside the built-in one rather than replacing it.
 *
 * Kept as a `computed` rather than an inline arrow so the dependency on the settings state
 * is explicit; both forms work with `useHead`.
 */
const faviconLinks = computed((): HeadIconLink[] => brandingIcons(faviconUrl.value).map((icon) => ({
  // unhead's link `rel` takes any string; widening the enum to `string` keeps every element the
  // same type, so the array is not inferred as a union of literal-`rel` objects that unhead's
  // discriminated link type rejects.
  rel: String(icon.rel),
  href: icon.href,
  ...(icon.type ? { type: icon.type } : {}),
  ...(icon.sizes ? { sizes: icon.sizes } : {}),
  ...(icon.color ? { color: icon.color } : {})
})))

/**
 * The operator's site name and tagline. `og:site_name` and `author` used to be static
 * strings in nuxt.config's head; they are set here because the settings are only known
 * at request time. A page's own `useSeo()` still wins for `description`, because it
 * runs after this and unhead lets the later tag replace the earlier one.
 */
/** Operator-set site name and tagline, as reactive refs. */
const { name: siteName, tagline: siteTagline } = useSiteIdentity()

useSeoMeta({
  ogSiteName: siteName,
  author: siteName,
  ...(siteTagline.value ? { description: siteTagline, ogDescription: siteTagline } : {})
})

useHead({
  htmlAttrs: {
    lang: () => resolveLanguage(locale.value)
  },
  link: faviconLinks,
  // The colour-mode class on <html> is owned by plugins/color-mode.ts; the live-chat
  // loader by plugins/live-chat.ts; the tag manager by plugins/tracking.client.ts.
  // Nothing in this file should add a script or touch documentElement.classList.
})
</script>

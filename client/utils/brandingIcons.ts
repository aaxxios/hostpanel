/**
 * Derives the full browser/iOS/Android icon set from the stored favicon URL.
 *
 * The `portal.favicon` setting holds one URL. When an operator uploaded the image, the API
 * generated a whole set beside it: every file of one upload lands in the same directory,
 * under names the backend chooses. This module is the storefront's half of that contract.
 *
 * The names are fixed by `BrandingImageProcessor.RenderFaviconSet` in
 * `backend/src/Innovayse.Hostpanel.Infrastructure/ImageProcessing/`. **Renaming a file there without changing
 * `UPLOADED_ICON_SET` here produces no error anywhere**: the tags are emitted, the browser
 * requests them, the proxy answers 404, and the tab silently keeps the previous icon. If you
 * touch one side, touch both.
 *
 * @module utils/brandingIcons
 */

import { BrandingIconRel } from '~/types/brandingiconrel'
import type { HeadIconLink } from '~/types/headiconlink'

/** One icon the storefront links, and how it is linked. */
export interface BrandingIcon {
  /** Value for the tag's `rel` attribute. */
  rel: BrandingIconRel
  /** Absolute-from-root URL of the file. */
  href: string
  /** Media type, so the browser can skip formats it cannot render. */
  type: string
  /** `WxH` for the `sizes` attribute; omitted for a link that takes no size. */
  sizes?: string
  /** Tint for a `mask-icon` silhouette; omitted for every other link. */
  color?: string
}

/**
 * Prefix every uploaded branding URL starts with.
 *
 * Membership of this path is what tells an uploaded favicon apart from a URL the operator
 * pasted by hand. A pasted URL points at a single file on someone else's server, so guessing
 * sibling names against it would emit a page full of broken icon links.
 */
const UPLOADS_PREFIX = '/uploads/branding/'

/** One file the backend generates beside an uploaded favicon. */
interface UploadedIconFile {
  /** File name inside the upload's directory. */
  file: string
  /** Value for the tag's `rel` attribute. */
  rel: BrandingIconRel
  /** `WxH` for the `sizes` attribute. */
  sizes?: string
}

/**
 * The files generated beside an uploaded favicon, and the tag each becomes.
 *
 * Mirrors the backend's `_faviconSet`, minus the sizes no tag references.
 */
const UPLOADED_ICON_SET: readonly UploadedIconFile[] = [
  { file: 'favicon-16x16.png', rel: BrandingIconRel.Icon, sizes: '16x16' },
  { file: 'favicon-32x32.png', rel: BrandingIconRel.Icon, sizes: '32x32' },
  { file: 'apple-touch-icon.png', rel: BrandingIconRel.AppleTouchIcon, sizes: '180x180' },
  { file: 'icon-192.png', rel: BrandingIconRel.Icon, sizes: '192x192' },
  { file: 'icon-512.png', rel: BrandingIconRel.Icon, sizes: '512x512' },
]

/**
 * The icons a fresh install serves, straight out of `public/`.
 *
 * All of them exist on disk, so the iOS icon and the PWA install prompt the web manifest
 * advertises never point at a 404. The PNGs are rendered from the brand kit in `docs/brand/`
 * (the mark and the app tile); regenerate them from it if the mark ever changes.
 *
 * They live here rather than in `nuxt.config.ts` because two places emitting
 * `<link rel="icon">` is one too many: unhead only dedupes link tags that carry a matching
 * explicit key, so a static tag in the config and an uploaded one from `app.vue` would both
 * survive into the rendered head, and Chrome prefers the SVG: an operator who uploaded a
 * favicon would keep seeing the built-in mark.
 */
const BUILT_IN_ICONS: BrandingIcon[] = [
  { rel: BrandingIconRel.Icon, href: '/favicon.ico', type: 'image/x-icon' },
  // The vector mark, for browsers that render it crisply at any tab size.
  { rel: BrandingIconRel.Icon, href: '/favicon.svg', type: 'image/svg+xml' },
  { rel: BrandingIconRel.Icon, href: '/favicon-16x16.png', type: 'image/png', sizes: '16x16' },
  { rel: BrandingIconRel.Icon, href: '/favicon-32x32.png', type: 'image/png', sizes: '32x32' },
  { rel: BrandingIconRel.Icon, href: '/favicon-96x96.png', type: 'image/png', sizes: '96x96' },
  { rel: BrandingIconRel.AppleTouchIcon, href: '/apple-touch-icon.png', type: 'image/png', sizes: '180x180' },
  { rel: BrandingIconRel.Icon, href: '/icon-192.png', type: 'image/png', sizes: '192x192' },
  { rel: BrandingIconRel.Icon, href: '/icon-512.png', type: 'image/png', sizes: '512x512' },
  // Built-in only: an upload has no silhouette, so a pinned tab must not keep showing this one.
  { rel: BrandingIconRel.MaskIcon, href: '/safari-pinned-tab.svg', type: '', color: '#5E8EF3' },
]

/**
 * Whether a favicon URL points at a file this deployment generated the whole set for.
 *
 * The prefix alone is not enough. Uploads made before the icon generator existed sit flat in
 * the uploads root (`/uploads/branding/favicon-<guid>.png`), and the generated ones sit one
 * directory down (`/uploads/branding/<kind>/<id>/<file>`). Only the second kind has siblings,
 * so accepting the first would put five invented links into the page head, each one a 404.
 *
 * @param url The stored `portal.favicon` value.
 * @returns True when the URL names a file inside a generated icon directory.
 */
export const isUploadedBranding = (url: string): boolean => {
  if (!url.startsWith(UPLOADS_PREFIX)) {
    return false
  }

  // kind/id/file is three segments; a legacy flat upload is one.
  return url.slice(UPLOADS_PREFIX.length).split('/').length >= 3
}

/**
 * Builds the icon links for a stored favicon URL.
 *
 * @param faviconUrl The stored `portal.favicon` value; empty when nothing is set.
 * @returns
 *   The links to emit. Empty when nothing is set, so the caller leaves `nuxt.config`'s static
 *   defaults in place. A single link for a pasted URL, because only that one file is known to
 *   exist. The full set for an upload.
 */
export const brandingIcons = (faviconUrl: string): BrandingIcon[] => {
  if (!faviconUrl) {
    return BUILT_IN_ICONS
  }

  if (!isUploadedBranding(faviconUrl)) {
    // Someone else's file. It may be an .ico, an .svg or a .png: the browser will work it
    // out, and declaring a type we have not checked would be worse than declaring none.
    return [{ rel: BrandingIconRel.Icon, href: faviconUrl, type: '' }]
  }

  const directory = faviconUrl.slice(0, faviconUrl.lastIndexOf('/') + 1)

  return UPLOADED_ICON_SET.map(icon => ({
    rel: icon.rel,
    href: `${directory}${icon.file}`,
    type: 'image/png',
    sizes: icon.sizes,
  }))
}

/**
 * Turns one branding icon into the `<link>` shape `useHead` accepts.
 *
 * unhead types links as a union keyed by `rel`, so each `rel` is built in its own branch: an
 * object whose `rel` is still the whole enum would match none of unhead's members. Empty
 * `type`/`sizes` are dropped rather than emitted as empty attributes.
 *
 * @param icon One icon from {@link brandingIcons}.
 * @returns The head link for that icon.
 */
export const toHeadIconLink = (icon: BrandingIcon): HeadIconLink => {
  if (icon.rel === BrandingIconRel.MaskIcon) {
    // Only the built-in silhouette is a mask icon, and it always carries its tint.
    return { rel: icon.rel, href: icon.href, color: icon.color ?? '' }
  }

  /** The optional attributes, present only when they hold a value. */
  const attributes = {
    href: icon.href,
    ...(icon.type ? { type: icon.type } : {}),
    ...(icon.sizes ? { sizes: icon.sizes } : {})
  }

  // Same body as the favicon return below, but a separate branch: it narrows `rel` to one
  // value, which is what makes the object match exactly one member of the union.
  if (icon.rel === BrandingIconRel.AppleTouchIcon) {
    return { rel: icon.rel, ...attributes }
  }

  return { rel: icon.rel, ...attributes }
}

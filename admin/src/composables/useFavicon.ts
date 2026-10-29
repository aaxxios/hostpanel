/**
 * Points the browser tab at the operator's favicon.
 *
 * `index.html` ships two `<link rel="icon">` tags for the built-in mark, because a tab
 * needs an icon before any JavaScript has run. This replaces them once the branding
 * settings arrive: a static SPA has no server render to write them into, so the swap
 * has to happen in the document.
 *
 * The built-in tags are removed rather than added to. A browser handed two `rel="icon"` links
 * picks one by its own rules, and Chrome prefers the SVG, so leaving the built-in tag in
 * place would fetch an uploaded PNG favicon and then ignore it.
 */

import { watch } from 'vue'
import { useBrandingStore } from '@/stores/branding'

/** Marks the tags this module owns, so a second run replaces rather than accumulates. */
const OWNED = 'data-branding-favicon'

/**
 * Applies a favicon URL to the document, replacing whatever icon links are there.
 *
 * @param url - Absolute or root-relative favicon URL. An empty string is ignored, which
 * leaves `index.html`'s built-in links untouched.
 */
const applyFavicon = (url: string): void => {
  if (!url) {
    return
  }

  // The built-in pinned-tab silhouette goes too: an upload has none, and Safari would
  // otherwise keep pinning the shipped mark for an operator's own brand.
  document.head.querySelectorAll('link[rel~="icon"], link[rel="mask-icon"]').forEach(el => el.remove())

  const link = document.createElement('link')
  link.rel = 'icon'
  link.href = url
  link.setAttribute(OWNED, '')
  document.head.appendChild(link)
}

/**
 * Loads the branding settings and keeps the tab icon in step with them.
 *
 * Call once, from the root component. Safe to call again: the store only fetches once.
 */
export const useFavicon = (): void => {
  const branding = useBrandingStore()

  void branding.load()

  watch(
    () => branding.faviconUrl,
    url => applyFavicon(url),
    { immediate: true },
  )
}

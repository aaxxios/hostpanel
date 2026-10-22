/**
 * Shared name of the CSS variable the cookie banner publishes its measured height under.
 *
 * The banner writes it on the root element while it is visible and removes it on dismissal. The
 * floating contact button, which sits where the banner does, reads it in its class
 * (`bottom-[calc(1.5rem+var(--cookie-banner-height,0px))]`) and lifts clear of the card only while
 * the card is shown. The banner's in-flow spacer reads it too (`h-[var(--cookie-banner-height,0px)]`),
 * but only on a phone (`sm:hidden`), where the full-width card covers the end of the page; from `sm` it
 * would be an empty band under the footer. Tailwind needs those class names as literals, so they repeat
 * this name and the banner test pins them together.
 *
 * @module utils/ui/cookieBanner
 */

/** Name of the CSS custom property holding the banner's height in pixels. */
export const COOKIE_BANNER_HEIGHT_VAR = '--cookie-banner-height'

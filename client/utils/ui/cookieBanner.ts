/**
 * Shared name of the CSS variable the cookie banner publishes its measured height under.
 *
 * The banner writes it on the root element while it is visible and removes it on dismissal. The
 * in-flow spacer and the floating contact button read it in their classes
 * (`h-[var(--cookie-banner-height,0px)]`, `bottom-[calc(1.5rem+var(--cookie-banner-height,0px))]`);
 * Tailwind needs those class names as literals, so they repeat this name and the banner test
 * pins the two together.
 *
 * @module utils/ui/cookieBanner
 */

/** Name of the CSS custom property holding the banner's height in pixels. */
export const COOKIE_BANNER_HEIGHT_VAR = '--cookie-banner-height'

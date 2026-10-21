/**
 * Class strings the aurora template paints its bare buttons with.
 *
 * `UiButton` with `variant="bare"` keeps only structure and focus, so the template's own palette
 * comes from these literals. They are complete class names (Tailwind reads them from this file)
 * and live in one place so the brand button is the same in the hero, the plan cards, the search
 * bar and the checkout.
 *
 * @module templates/aurora/buttons
 */

/**
 * Frame of a header control (language, currency, theme, cart): one height, radius, border and
 * hover for all four, in both colour modes. Add `w-10` for an icon-only control or `gap-2 px-3`
 * for one with text.
 */
export const HEADER_CONTROL = 'inline-flex h-10 items-center justify-center rounded-xl border border-line2 bg-transparent text-tx transition-colors hover:border-ac1 hover:text-ac1'

/**
 * A header text link (navigation entry, "Sign in"): primary text colour, a brand-coloured underline
 * that grows in on hover and stays on the current entry.
 */
export const NAV_LINK = "relative rounded-lg px-2 py-1 text-tx transition-colors hover:text-ac1 after:absolute after:inset-x-2 after:-bottom-0.5 after:h-0.5 after:origin-left after:scale-x-0 after:rounded-full after:bg-ac1 after:transition-transform after:content-[''] hover:after:scale-x-100"

/** The current header entry: brand colour, bolder, with the underline shown. */
export const NAV_LINK_ACTIVE = 'font-semibold text-ac1 after:scale-x-100'

/** Filled brand-gradient button; the text is white in both colour modes. */
export const BRAND_BUTTON = 'rounded-xl bg-brand font-bold text-on-brand hover:brightness-110 disabled:cursor-not-allowed disabled:opacity-40'

/** Framed button on the template line colour that lights up on hover. */
export const LINE_BUTTON = 'rounded-xl border border-line2 font-semibold text-tx hover:border-ac1 hover:text-ac1'

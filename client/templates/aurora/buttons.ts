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

/** Filled brand-gradient button; the text is white in both colour modes. */
export const BRAND_BUTTON = 'rounded-xl bg-brand font-bold text-on-brand hover:brightness-110 disabled:cursor-not-allowed disabled:opacity-40'

/** Framed button on the template line colour that lights up on hover. */
export const LINE_BUTTON = 'rounded-xl border border-line2 font-semibold text-tx hover:border-ac1 hover:text-ac1'

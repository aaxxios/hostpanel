/**
 * Colours and type of the payment provider's card input.
 *
 * The card input draws itself inside a frame owned by the provider, so Tailwind classes and CSS
 * variables of the page can not reach it: its text colours have to be handed over as literal
 * values. They live here, away from the component, so the one place that needs raw colours is
 * small and testable, and so a screen never carries a hex value.
 *
 * @module utils/stripeCardStyle
 */

/** Style of the placeholder text inside the card field. */
export interface StripePlaceholderStyle {
  /** Text colour of the placeholder. */
  color: string
}

/** The style object the card element accepts (the part of the provider's type this module fills). */
export interface StripeCardStyle {
  /** Style of the field in its normal state. */
  base: {
    /** Text colour. */
    color: string
    /** Font stack. */
    fontFamily: string
    /** Font size with unit. */
    fontSize: string
    /** Style of the placeholder text. */
    '::placeholder': StripePlaceholderStyle
  }
  /** Style of the field while its content is invalid. */
  invalid: {
    /** Text colour. */
    color: string
    /** Colour of the error icon. */
    iconColor: string
  }
}

/** Text colour on the dark surface. */
const TEXT_ON_DARK = '#ffffff'

/** Text colour on the light surface. */
const TEXT_ON_LIGHT = '#111827'

/** Placeholder colour; readable on both surfaces. */
const PLACEHOLDER = '#6b7280'

/** Error colour on the dark surface. */
const INVALID_ON_DARK = '#f87171'

/** Error colour on the light surface. */
const INVALID_ON_LIGHT = '#dc2626'

/**
 * Builds the card element's inner text style for a colour mode.
 *
 * @param dark - Whether the page is in dark mode.
 * @returns The style object accepted by the card element.
 */
export const buildStripeCardStyle = (dark: boolean): StripeCardStyle => ({
  base: {
    color: dark ? TEXT_ON_DARK : TEXT_ON_LIGHT,
    fontFamily: '"Inter", system-ui, sans-serif',
    fontSize: '16px',
    '::placeholder': { color: PLACEHOLDER },
  },
  invalid: {
    color: dark ? INVALID_ON_DARK : INVALID_ON_LIGHT,
    iconColor: dark ? INVALID_ON_DARK : INVALID_ON_LIGHT,
  },
})

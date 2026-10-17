/**
 * The two billing cycles the backend prices.
 *
 * The member values are the keys of `PortalProductPricing` (`monthly`, `annual`), so a cycle can
 * index a product's `pricing` directly. The i18n key for a cycle is spelled differently (`annually`)
 * and is mapped by the page that shows it.
 *
 * @module types/pricingcycle
 */

/** A billing cycle a plan is priced in. */
export enum PricingCycle {
  /** Billed every month. */
  Monthly = 'monthly',
  /** Billed once a year. */
  Annual = 'annual',
}

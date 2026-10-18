/**
 * A renderable position a template must supply.
 *
 * `header` and `footer` are resolved by the layout; the rest are page bodies.
 *
 * @module types/templateslot
 */
export enum TemplateSlot {
  /** The site header. */
  Header = 'header',
  /** The site footer. */
  Footer = 'footer',
  /** The home page body. */
  Home = 'home',
  /** The hosting page body. */
  Hosting = 'hosting',
  /** The domains page body. */
  Domains = 'domains',
  /** The checkout page body. */
  Checkout = 'checkout'
}

/**
 * Icons of the classic header's Support dropdown.
 *
 * The Hosting dropdown takes its icons from the shared product-type map (`productTypeIcon`), so the
 * header and the product cards cannot disagree. Support links are not products, so their icons
 * live here, in one place for the dropdown and the mobile drawer. Every name is spelled out in full
 * on purpose: the icon module bundles the names it finds as literals in the source.
 *
 * @module constants/headerNavIcon
 */

/** Keys of the Support dropdown links. */
export enum SupportLinkKey {
  /** The knowledge base. */
  Help = 'help',
  /** The frequently asked questions. */
  Faq = 'faq',
  /** The contact page. */
  Contact = 'contact',
}

/** Icon of each Support link. */
export const SUPPORT_LINK_ICON: Readonly<Record<SupportLinkKey, string>> = {
  [SupportLinkKey.Help]: 'mdi:book-open-variant',
  [SupportLinkKey.Faq]: 'mdi:frequently-asked-questions',
  [SupportLinkKey.Contact]: 'mdi:headset',
}

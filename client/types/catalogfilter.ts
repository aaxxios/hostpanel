/**
 * The catalogue filters a caller chooses.
 *
 * @module types/catalogfilter
 */

import type { PortalProductQuery } from '~/types/portalproductquery'

/**
 * The catalogue filters a caller chooses.
 *
 * The currency is left out on purpose: `stores/catalog.ts` always requests the payer's own
 * currency, so a page cannot ask for the wrong one.
 */
export type CatalogFilter = Omit<PortalProductQuery, 'currency'>

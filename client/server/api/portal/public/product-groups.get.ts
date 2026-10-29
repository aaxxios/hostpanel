/**
 * GET /api/portal/public/product-groups
 *
 * The catalogue's active product groups; the storefront shows each one as a product family.
 * `internalApiCall` forwards `X-Language` as on every route, but the names are the operator's own
 * text and come back as written, not translated.
 *
 * @module server/api/portal/public/product-groups.get
 */

import type { PortalProductGroup } from '~/types/portalproductgroup'

/** Handler: the backend's active product groups, passed through as sent. */
export default defineEventHandler(async (event): Promise<PortalProductGroup[]> =>
  await internalApiCall<PortalProductGroup[]>(event, '/public/product-groups'))

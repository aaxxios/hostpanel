/**
 * GET /api/portal/public/products
 * Returns products from the C# backend.
 *
 * Query params: pid, gid, gids, currency. Every one is validated by `readCatalogQuery` before it
 * reaches the cache key or the backend address, so the key has a fixed shape and a visitor cannot
 * grow it with free text. The backend does the filtering (`productId`, `groupId`, `groupIds`); this
 * route only renames the filters to the catalogue's own parameter names.
 *
 * There is deliberately no `lang`: `CatalogProductDto` carries no translated fields and
 * `ProductsController` has no locale parameter, so the response is the same in every language.
 * Localised product copy is a backend change, a `ProductTranslation` entity beside the existing
 * `SlideTranslation`, and until it exists such a parameter could only mislead its caller.
 *
 * @module server/api/portal/public/products.get
 */

import type { H3Event } from 'h3'
import { catalogCacheKey, readCatalogQuery } from '~/server/utils/catalogQuery'
import { hasSessionCookie } from '~/server/utils/portal-session'
import type { PortalProduct } from '~/types/portalproduct'

/**
 * The actual product fetch: pulled out so a signed-in caller can bypass the shared cache
 * below entirely (see the wrapper's note).
 *
 * @param event - The request event.
 * @returns The catalogue, filtered as asked, priced in the caller's own currency.
 */
const loadProducts = async (event: H3Event): Promise<PortalProduct[]> => {
  const { pid, gid, gids, currency } = readCatalogQuery(event)
  const params = new URLSearchParams()

  if (pid !== undefined) {
    params.set('productId', String(pid))
  }
  if (gid !== undefined) {
    params.set('groupId', String(gid))
  }
  for (const groupId of gids ?? []) {
    params.append('groupIds', String(groupId))
  }

  // Anonymous callers may ask for prices in a specific enabled currency (the storefront's
  // currency picker): forwarded straight through, same as `tld-pricing.get.ts` does.
  // `ListCatalogProductsHandler` ignores it for a signed-in caller.
  if (currency !== undefined) {
    params.set('currency', currency)
  }

  const qs = params.toString()

  // Every product is forwarded exactly as `CatalogProductDto` sends it: `pricing` is already the
  // amount in the caller's own currency (their client record's, the base currency for a guest
  // with no `currency` request, or the requested enabled currency otherwise). See the note on
  // `types/portalproduct.ts`.
  return await internalApiCall<PortalProduct[]>(event, `/public/products${qs ? `?${qs}` : ''}`)
}

/** The shared-cache path, reached only by guests: one entry per (filters, currency) pair. */
const cachedLoadProducts = defineCachedEventHandler(loadProducts, {
  name: 'backend-products',
  maxAge: 3600,
  swr: true,
  // The key carries no locale: a page's language is not the right thing to price by, and
  // this cached path is only ever reached anonymously (see the wrapper below). It DOES carry
  // `currency`: an anonymous caller can request pricing in any enabled currency, and a
  // response priced in USD must never be served back to a request that asked for AMD (or vice
  // versa): the entries below are one cache per (filters, currency) pair. Built from the
  // validated query only: integers, a bounded sorted list and a three-letter code.
  getKey: (event) => catalogCacheKey(readCatalogQuery(event))
})

/**
 * A signed-in caller's products are priced in *their* currency, not the base currency the
 * cached path above answers with: sharing that cache across every signed-in client would
 * hand one client's prices to another the next time the key matched. So a request carrying an
 * auth cookie skips the cache and calls the backend directly; only a guest's request, always
 * priced in the base currency, is safe to share.
 */
export default defineEventHandler(async (event) => {
  if (hasSessionCookie(event)) {
    return await loadProducts(event)
  }
  return await cachedLoadProducts(event)
})

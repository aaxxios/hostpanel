/**
 * One catalogue product, as `GET /api/portal/public/products` returns it.
 *
 * The route forwards the C# `CatalogProductDto` verbatim: every field below is its own,
 * camelCase, as `System.Text.Json` writes it, including `pricing` and `prices`. The backend
 * prices only monthly and annually, and `pricing` already arrives in the caller's own currency
 * (the signed-in client's, or the base currency for a guest): see
 * `Innovayse.Hostpanel.Application.Products.Common.ProductPricingDto` and
 * `PayerCurrencyResolver.ForCallerAsync`. Neither a fixed currency nor the page's language
 * decides it.
 *
 * ## `groupId`, not `gid`
 *
 * `ProductDto` sends `groupId`. The BFF's `gid` is a *query* parameter it translates to
 * `groupId` on the way out, not a response field, so reading `gid` off a product would give
 * `NaN` and no plan would ever be grouped into a family.
 *
 * ## There are no `translated_*` fields
 *
 * `ProductDto` carries no `translated_*`, `group_translations` or `group_features` fields,
 * and the BFF forwards no `lang`: see `server/api/portal/public/products.get.ts`.
 *
 * **Localised product copy does not exist in the backend.** There is no product translations
 * table, no `lang` on `ProductsController`, nothing under `Application/Products/`. `Slide` is
 * the one entity with a `*Translation` sibling (`SlideTranslation`), and
 * `useCatalogStore().loadSlides(lang)` is the one place a `lang` parameter still means anything.
 * Making product copy translatable starts there, a `ProductTranslation` entity and a locale
 * on the query, and only then does a field belong back on this interface.
 *
 * **`group_features` already has a successor.** Structured, per-product specification lines
 * are `ProductFeature` (label / value / sortOrder), served by
 * `GET /api/portal/public/product-features` and read through
 * `useCatalogStore().loadProductFeatures(groupId)`. A caller that wanted `group_features` wants
 * that endpoint.
 *
 * @module types/portalproduct
 */

import type { PortalProductPricing } from '~/types/portalproductpricing'
import type { PortalProductCurrencyPrice } from '~/types/portalproductcurrencyprice'
import type { ProductType } from '~/types/producttype'

/** One catalogue product. */
export interface PortalProduct {
  /** Product primary key. */
  id: number
  /** Product group id this product belongs to. */
  groupId: number
  /** Product name. */
  name: string
  /** Marketing description; the feature bullets are parsed out of it. */
  description?: string | null
  /** Landing-page URL for the product, when it has one. */
  website?: string | null
  /** URL slug, when the product has one. */
  slug?: string | null
  /** Product type, as `ProductType` serialises. */
  type?: ProductType
  /** Display order, ascending, when the API supplies one; plans fall back to price order without it. */
  sortOrder?: number | null
  /** True when the operator marked this product as its group's recommended plan. */
  isFeatured?: boolean
  /** The product's price in the caller's own currency; null when it carries no price at all. */
  pricing?: PortalProductPricing | null
  /** The product's price in every currency it is sold in: what the admin pricing grid edits. */
  prices?: PortalProductCurrencyPrice[]
}

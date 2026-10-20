/**
 * One catalogue product, as `GET /api/portal/public/products` returns it.
 *
 * The route spreads the C# `ProductDto` verbatim and adds exactly one thing: `pid`, a
 * legacy-compatible alias of `id`. Every other field below is `ProductDto`'s own, camelCase, as
 * `System.Text.Json` writes it — including `pricing` and `prices`, which the route used to
 * replace with a hardcoded `pricing.USD` block of decimal strings under six billing-cycle
 * keys. The backend prices only monthly and annually, and `pricing` already arrives in the
 * caller's own currency (the signed-in client's, or the base currency for a guest) — see
 * `Innovayse.Application.Products.Common.ProductPricingDto` and
 * `PayerCurrencyResolver.ForCallerAsync`. Relabelling it `USD` and formatting it with `$` was
 * wrong for every currency but the base, and a page's language was never the right thing to
 * decide it either (`docs/superpowers/specs/2026-09-13-multi-currency-pricing-design.md` §5).
 *
 * ## `groupId`, not `gid`
 *
 * This declared `gid`, and the public site reads `Number(p.gid)` in four places to group plans
 * into product families. `ProductDto` has always sent `groupId`; nothing anywhere adds `gid` to
 * a product (the BFF's `gid` handling is a *query* parameter it translates to `groupId` on the
 * way out, not a response field). So `Number(p.gid)` was `NaN` on every product, no group ever
 * matched, and `components/sections/Products.vue` dropped every family it tried to build.
 *
 * ## There are no `translated_*` fields, and nothing reads them any more
 *
 * `translated_name`, `translated_tagline`, `translated_shortdescription`,
 * `translated_description`, `group_translations` and `group_features` used to be read across
 * `pages/products/`, `pages/hosting/`, `pages/trial/`, `components/sections/Products.vue`,
 * `templates/classic/pages/Hosting.vue` and `utils/catalogGroups.ts`. `ProductDto` carries none of
 * them, `GET /api/products` was checked against a live backend and sends none of them, and
 * every read sat in front of an `|| p.name` / `|| p.description` fallback that was the only
 * branch ever taken. They are gone, along with the `lang` parameter the BFF forwarded to feed
 * them — see `server/api/portal/public/products.get.ts`.
 *
 * **Localised product copy does not exist in the backend.** There is no product translations
 * table, no `lang` on `ProductsController`, nothing under `Application/Products/`. `Slide` is
 * the one entity with a `*Translation` sibling (`SlideTranslation`), and
 * `useCatalogStore().loadSlides(lang)` is the one place a `lang` parameter still means anything.
 * Making product copy translatable starts there — a `ProductTranslation` entity and a locale
 * on the query — and only then does a field belong back on this interface.
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

/** One catalogue product. */
export interface PortalProduct {
  /** Product primary key. */
  id: number
  /** legacy-compatible alias of {@link PortalProduct.id}, added by the proxy. */
  pid?: number
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
  /** Hosting package name used for provisioning. */
  packageName?: string | null
  /** Product type, as `ProductType` serialises. */
  type?: string
  /** Current status, as `ProductStatus` serialises. */
  status?: string
  /** FK to the server group used for provisioning. */
  serverGroupId?: number | null
  /** Display order, ascending, when the API supplies one; plans fall back to price order without it. */
  sortOrder?: number | null
  /** The product's price in the caller's own currency; null when it carries no price at all. */
  pricing?: PortalProductPricing | null
  /** The product's price in every currency it is sold in — what the admin pricing grid edits. */
  prices?: PortalProductCurrencyPrice[]
}

/**
 * One currency, as `GET /api/portal/public/currencies` lists it.
 *
 * The endpoint proxies `/api/reference/currencies`, which returns `CurrencyList.All`: the
 * static ISO 4217 reference table in `Innovayse.Hostpanel.Application.Admin.Common`, not the operator's
 * configured billing currencies. `CurrencyDto` is `(string Code, string Name, string Symbol)`
 * and that is the whole of it. `utils/formatCurrency.ts` renders an amount from the ISO code.
 *
 * @module types/portalcurrency
 */

/** One currency from the ISO 4217 reference table. */
export interface PortalCurrency {
  /** ISO 4217 code, e.g. `USD`. */
  code: string
  /** English display name, e.g. `US Dollar`. */
  name: string
  /** Currency symbol, e.g. `$`. */
  symbol: string
}

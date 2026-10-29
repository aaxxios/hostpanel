/** The validated filters of a public catalogue request, as the server reads them from the query string. */
export interface CatalogQuery {
  /** The one product asked for, when the request names one. */
  pid?: number
  /** The one product group asked for, when the request names one. */
  gid?: number
  /** The product groups asked for, sorted and without duplicates; undefined when the request names none. */
  gids?: number[]
  /** The ISO 4217 code of the currency to price in, upper case; undefined for the caller's own. */
  currency?: string
}

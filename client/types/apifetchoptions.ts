/**
 * Options of the one-shot `apiFetch` call.
 *
 * @module types/apifetchoptions
 */
import type { HttpMethod } from '~/types/httpmethod'

/** `$fetch`'s own options, with the verb narrowed to the closed {@link HttpMethod} set. */
export type ApiFetchOptions = Omit<NonNullable<Parameters<typeof $fetch>[1]>, 'method'> & {
  /** HTTP verb; GET when absent. */
  method?: HttpMethod
}

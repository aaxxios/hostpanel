/**
 * The endpoint set every storefront content type shares: list, create, update, delete and
 * reorder under an `/admin/...` route root.
 *
 * No state lives here; every function rejects with an `ApiError` carrying the problem+json body.
 * The per-type composables (`useTestimonialsApi`, `usePartnersApi`, `useProjectsApi`,
 * `useFaqsApi`) fix the route root and the payload types.
 */

import { useApi } from '@/composables/useApi'
import type { StorefrontOrderItem } from '@/types/storefront'
import { HttpMethod } from '@/types/http'
import type { ApiPath } from '@/lib/apiPaths'

/** The endpoint functions of one storefront content type. */
export interface StorefrontApi<TItem, TPayload, TUpdatePayload = TPayload> {
  /** Lists every entry with its translations. */
  list: () => Promise<TItem[]>
  /** Creates an entry and answers its id. */
  create: (payload: TPayload) => Promise<number>
  /** Updates an entry by id. */
  update: (id: number, payload: TUpdatePayload) => Promise<void>
  /** Deletes an entry by id. */
  remove: (id: number) => Promise<void>
  /** Saves the display order of several entries. */
  reorder: (items: StorefrontOrderItem[]) => Promise<void>
}

/**
 * Endpoint functions for one `/admin/...` route root.
 *
 * @param base - Route root of the content type, e.g. `ApiPath.AdminTestimonials`.
 * @returns One typed function per endpoint.
 */
export const useStorefrontApi = <TItem, TPayload, TUpdatePayload = TPayload>(
  base: ApiPath,
): StorefrontApi<TItem, TPayload, TUpdatePayload> => {
  const { request } = useApi()

  /**
   * Lists every entry.
   *
   * @returns Every entry with its translations.
   */
  const list = (): Promise<TItem[]> => request<TItem[]>(base)

  /**
   * Creates an entry.
   *
   * @param payload - Entry data with translations.
   * @returns The new entry's id.
   */
  const create = (payload: TPayload): Promise<number> =>
    request<number>(base, { method: HttpMethod.Post, body: JSON.stringify(payload) })

  /**
   * Updates an entry.
   *
   * @param id - Entry id.
   * @param payload - Updated entry data.
   * @returns Nothing; the API answers 204.
   */
  const update = (id: number, payload: TUpdatePayload): Promise<void> =>
    request<void>(`${base}/${id}`, { method: HttpMethod.Put, body: JSON.stringify(payload) })

  /**
   * Deletes an entry.
   *
   * @param id - Entry id.
   * @returns Nothing; the API answers 204.
   */
  const remove = (id: number): Promise<void> => request<void>(`${base}/${id}`, { method: HttpMethod.Delete })

  /**
   * Saves the display order of several entries.
   *
   * @param items - Entry id and new sort order pairs.
   * @returns Nothing; the API answers 204.
   */
  const reorder = (items: StorefrontOrderItem[]): Promise<void> =>
    request<void>(`${base}/order`, { method: HttpMethod.Put, body: JSON.stringify({ items }) })

  return { list, create, update, remove, reorder }
}

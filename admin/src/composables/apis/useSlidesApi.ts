/**
 * Endpoints behind the slides store: homepage slide CRUD and ordering.
 *
 * The transport is the shared storefront endpoint set; this file only gives the slide payload
 * types and the slide-named functions the store calls. The image upload is in `useUploadApi`.
 * Every function rejects with an `ApiError` carrying the problem+json body.
 */

import { useStorefrontApi } from '@/composables/apis/useStorefrontApi'
import { ApiPath } from '@/lib/apiPaths'
import type { CreateSlidePayload, Slide, SlideOrderItem, UpdateSlidePayload } from '@/types/slide'

/**
 * Endpoint functions for `/admin/slides`.
 *
 * @returns One typed function per endpoint.
 */
export const useSlidesApi = () => {
  const api = useStorefrontApi<Slide, CreateSlidePayload, UpdateSlidePayload>(ApiPath.AdminSlides)

  /**
   * Saves the display order of several slides.
   *
   * @param items - Slide id and new sort order pairs.
   * @returns Nothing; the API answers 204.
   */
  const updateSlideOrder = (items: SlideOrderItem[]): Promise<void> => api.reorder(items)

  return {
    listSlides: api.list,
    createSlide: api.create,
    updateSlide: api.update,
    deleteSlide: api.remove,
    updateSlideOrder,
  }
}

/**
 * Endpoints behind the slides store: homepage slide CRUD and ordering.
 *
 * No state lives here; every function rejects with an `ApiError` carrying the problem+json body.
 * The image upload is in `useUploadApi`.
 */

import { useApi } from '@/composables/useApi'
import type { CreateSlidePayload, Slide, SlideOrderItem, UpdateSlidePayload } from '@/types/slide'

/**
 * Endpoint functions for `/admin/slides`.
 *
 * @returns One typed function per endpoint.
 */
export const useSlidesApi = () => {
  const { request } = useApi()

  /**
   * Lists every slide.
   *
   * @returns Every slide with its translations.
   */
  const listSlides = (): Promise<Slide[]> => request<Slide[]>('/admin/slides')

  /**
   * Creates a slide.
   *
   * @param payload - Slide data with translations.
   * @returns The new slide's id.
   */
  const createSlide = (payload: CreateSlidePayload): Promise<number> =>
    request<number>('/admin/slides', { method: 'POST', body: JSON.stringify(payload) })

  /**
   * Updates a slide.
   *
   * @param id - Slide id.
   * @param payload - Updated slide data.
   * @returns Nothing; the API answers 204.
   */
  const updateSlide = (id: number, payload: UpdateSlidePayload): Promise<void> =>
    request<void>(`/admin/slides/${id}`, { method: 'PUT', body: JSON.stringify(payload) })

  /**
   * Deletes a slide.
   *
   * @param id - Slide id.
   * @returns Nothing; the API answers 204.
   */
  const deleteSlide = (id: number): Promise<void> =>
    request<void>(`/admin/slides/${id}`, { method: 'DELETE' })

  /**
   * Saves the display order of several slides.
   *
   * @param items - Slide id and new sort order pairs.
   * @returns Nothing; the API answers 204.
   */
  const updateSlideOrder = (items: SlideOrderItem[]): Promise<void> =>
    request<void>('/admin/slides/order', { method: 'PUT', body: JSON.stringify({ items }) })

  return { listSlides, createSlide, updateSlide, deleteSlide, updateSlideOrder }
}

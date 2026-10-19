/**
 * Endpoints behind the testimonials store: `/admin/testimonials` CRUD and ordering.
 *
 * No state lives here; every function rejects with an `ApiError` carrying the problem+json body.
 * The image upload is in `useUploadApi`.
 */

import { useStorefrontApi } from '@/composables/apis/useStorefrontApi'
import { ApiPath } from '@/lib/apiPaths'
import type { StorefrontApi } from '@/composables/apis/useStorefrontApi'
import type { Testimonial, TestimonialPayload } from '@/types/testimonial'

/**
 * Endpoint functions for `/admin/testimonials`.
 *
 * @returns List, create, update, remove and reorder.
 */
export const useTestimonialsApi = (): StorefrontApi<Testimonial, TestimonialPayload> => useStorefrontApi<Testimonial, TestimonialPayload>(ApiPath.AdminTestimonials)

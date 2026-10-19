/**
 * GET /api/portal/public/testimonials
 *
 * The active client testimonials, in display order. The language reaches the backend as
 * `X-Language` (stamped by `internalApiCall`), so the quotes and roles come back localised.
 *
 * @module server/api/portal/public/testimonials.get
 */

import type { Testimonial } from '~/types/testimonial'

/** Handler: the backend's active testimonials, passed through as sent. */
export default defineEventHandler(async (event): Promise<Testimonial[]> =>
  await internalApiCall<Testimonial[]>(event, '/public/testimonials'))

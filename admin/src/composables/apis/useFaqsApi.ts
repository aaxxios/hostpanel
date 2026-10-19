/**
 * Endpoints behind the FAQ store: `/admin/faqs` CRUD and ordering.
 *
 * No state lives here; every function rejects with an `ApiError` carrying the problem+json body.
 */

import { useStorefrontApi } from '@/composables/apis/useStorefrontApi'
import { ApiPath } from '@/lib/apiPaths'
import type { StorefrontApi } from '@/composables/apis/useStorefrontApi'
import type { Faq, FaqPayload } from '@/types/faq'

/**
 * Endpoint functions for `/admin/faqs`.
 *
 * @returns List, create, update, remove and reorder.
 */
export const useFaqsApi = (): StorefrontApi<Faq, FaqPayload> => useStorefrontApi<Faq, FaqPayload>(ApiPath.AdminFaqs)

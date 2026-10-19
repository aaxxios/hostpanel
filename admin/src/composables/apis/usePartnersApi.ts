/**
 * Endpoints behind the partners store: `/admin/partners` CRUD and ordering.
 *
 * No state lives here; every function rejects with an `ApiError` carrying the problem+json body.
 * The image upload is in `useUploadApi`.
 */

import { useStorefrontApi } from '@/composables/apis/useStorefrontApi'
import { ApiPath } from '@/lib/apiPaths'
import type { StorefrontApi } from '@/composables/apis/useStorefrontApi'
import type { Partner, PartnerPayload } from '@/types/partner'

/**
 * Endpoint functions for `/admin/partners`.
 *
 * @returns List, create, update, remove and reorder.
 */
export const usePartnersApi = (): StorefrontApi<Partner, PartnerPayload> => useStorefrontApi<Partner, PartnerPayload>(ApiPath.AdminPartners)

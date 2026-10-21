/**
 * Endpoints behind the locations store: `/admin/locations` CRUD and ordering.
 *
 * No state lives here; every function rejects with an `ApiError` carrying the problem+json body.
 */

import { useStorefrontApi } from '@/composables/apis/useStorefrontApi'
import { ApiPath } from '@/lib/apiPaths'
import type { StorefrontApi } from '@/composables/apis/useStorefrontApi'
import type { ServerLocation, ServerLocationPayload } from '@/types/serverlocation'

/**
 * Endpoint functions for `/admin/locations`.
 *
 * @returns List, create, update, remove and reorder.
 */
export const useLocationsApi = (): StorefrontApi<ServerLocation, ServerLocationPayload> => useStorefrontApi<ServerLocation, ServerLocationPayload>(ApiPath.AdminLocations)

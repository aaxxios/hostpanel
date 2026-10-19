/**
 * Endpoints behind the projects store: `/admin/projects` CRUD and ordering.
 *
 * No state lives here; every function rejects with an `ApiError` carrying the problem+json body.
 * The image upload is in `useUploadApi`.
 */

import { useStorefrontApi } from '@/composables/apis/useStorefrontApi'
import { ApiPath } from '@/lib/apiPaths'
import type { StorefrontApi } from '@/composables/apis/useStorefrontApi'
import type { Project, ProjectPayload } from '@/types/project'

/**
 * Endpoint functions for `/admin/projects`.
 *
 * @returns List, create, update, remove and reorder.
 */
export const useProjectsApi = (): StorefrontApi<Project, ProjectPayload> => useStorefrontApi<Project, ProjectPayload>(ApiPath.AdminProjects)

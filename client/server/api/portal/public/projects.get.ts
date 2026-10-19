/**
 * GET /api/portal/public/projects
 *
 * The active portfolio projects, in display order. The language reaches the backend as
 * `X-Language` (stamped by `internalApiCall`).
 *
 * @module server/api/portal/public/projects.get
 */

import type { Project } from '~/types/project'

/** Handler: the backend's active projects, passed through as sent. */
export default defineEventHandler(async (event): Promise<Project[]> =>
  await internalApiCall<Project[]>(event, '/public/projects'))

/**
 * POST /api/portal/client/tickets
 * Opens a new support ticket via the C# backend.
 *
 * Body: { departmentId, subject, message, priority }: the backend's `CreateTicketCommand`
 */

import { HttpMethod } from '~/types/httpmethod'

export default defineEventHandler(async (event) => {
  const body = await readBody(event)
  return await internalApiCall<Record<string, unknown>>(event, '/me/tickets', {
    method: HttpMethod.Post,
    body,
  })
})

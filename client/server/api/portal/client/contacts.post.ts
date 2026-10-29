/**
 * POST /api/portal/client/contacts
 * Adds a new sub-contact to the client's account via the C# backend and answers with the new contact's id (the backend replies 201).
 */

import { HttpMethod } from '~/types/httpmethod'

export default defineEventHandler(async (event) => {
  const body = await readBody(event)
  return await internalApiCall<number>(event, '/clients/me/contacts', {
    method: HttpMethod.Post,
    body,
  })
})

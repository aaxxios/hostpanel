/**
 * POST /api/portal/public/domain-check
 * Checks domain availability via the C# backend.
 *
 * A missing or blank name is refused by the backend's validator (400 `VALIDATION_ERROR`, the
 * sentence in the reader's language), so there is no pre-check -- and no sentence -- here.
 */
export default defineEventHandler(async (event) => {
  const body = await readBody(event)
  const domain = typeof body?.domain === 'string' ? body.domain.trim() : ''

  return await internalApiCall<Record<string, unknown>>(event, '/domains/check', {
    method: 'POST',
    body: { domain },
  })
})

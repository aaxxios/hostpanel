/**
 * GET /api/portal/auth/email-verified
 *
 * Reads whether the signed-in account's email address is confirmed. `verified` is three-valued
 * on purpose: `null` means the credential carries no answer, and only an explicit `false` may
 * prompt the person: anything else nags callers whose token simply lacks the claim.
 */
export default defineEventHandler(async (event) => {
  return await internalApiCall<{ verified: boolean | null }>(event, '/auth/email-verified')
})

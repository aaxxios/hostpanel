/**
 * POST /api/portal/auth/2fa-setup
 * Issues a TOTP secret and the otpauth URI for the authenticated account.
 *
 * Enrolment only — two-factor stays off until `2fa-enable` is called with a code.
 *
 * @module server/api/portal/auth/2fa-setup.post
 */

/** What the backend answers a two-factor enrolment with. */
interface TwoFactorSetupResult {
  /** Base32 TOTP secret. */
  secret: string
  /** The `otpauth://` URI the authenticator app scans as a QR code. */
  qrCodeUri: string
}

/**
 * Issues the TOTP secret for the signed-in account.
 */
export default defineEventHandler(async (event) => {
  return await internalApiCall<TwoFactorSetupResult>(event, '/me/2fa/setup', {
    method: 'POST',
  })
})

/**
 * What starting two-factor enrolment gives the dialog to render.
 */
export interface TwoFactorSetup {
  /** Shared secret the visitor can type into an authenticator by hand. */
  secret: string
  /** The `otpauth:` URI the QR code encodes. */
  qrCodeUri: string
  /** The QR code rendered locally as a `data:` URL, never a remote image. */
  qrDataUrl: string
}

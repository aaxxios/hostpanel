/**
 * Which direction the two-factor dialog on the account page submits in.
 */
export enum TwoFactorMode {
  /** Shows the QR code and enrols the authenticator with a first code. */
  Enable = 'enable',
  /** Only collects a current code to switch two-factor off. */
  Disable = 'disable'
}

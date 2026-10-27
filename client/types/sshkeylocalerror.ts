/** Why a pasted key was refused in the browser, before any request was made. */
export enum SshKeyLocalError {
  /** The text is a private key; it is never sent. */
  PrivateKey = 'privateKey',
  /** The text is not an ED25519 public key line. */
  InvalidKey = 'invalidKey',
  /** No label was given. */
  LabelRequired = 'labelRequired'
}

/**
 * Client-side pre-checks for a pasted SSH public key. They give instant feedback and keep an
 * obviously wrong paste, above all a private key, from ever leaving the browser. The backend
 * re-validates everything; these checks are never the gate.
 *
 * @module utils/sshKey
 */

/** Shape of an OpenSSH ED25519 public key line: algorithm, 68-character base64 blob, optional comment. */
const ED25519_LINE = /^ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAI[A-Za-z0-9+/]{43}=?(?: [^\r\n]*)?$/

/** Longest label the backend accepts. */
const MAX_LABEL_LENGTH = 100

/**
 * Whether the text looks like a private key of any format. Such text must not be sent anywhere.
 *
 * @param text - The pasted text.
 * @returns True when it contains a private-key armour line.
 */
export const looksLikePrivateKey = (text: string): boolean => /PRIVATE KEY/.test(text)

/**
 * Whether the text is plausibly a single ED25519 public key line.
 *
 * @param text - The pasted text.
 * @returns True when it has the ED25519 public key shape.
 */
export const isEd25519PublicKey = (text: string): boolean => ED25519_LINE.test(text.trim())

/**
 * Suggests a label from the key's comment (usually `user@host`).
 *
 * @param text - The pasted key line.
 * @returns The comment, trimmed and capped at the label limit, or an empty string.
 */
export const suggestedLabel = (text: string): string => {
  const parts = text.trim().split(/\s+/)
  return parts.length >= 3 ? parts.slice(2).join(' ').slice(0, MAX_LABEL_LENGTH) : ''
}

/**
 * Encryption of the portal sessions at rest.
 *
 * A session holds the person's access and refresh tokens. In Redis they are kept as AES-256-GCM
 * ciphertext under a key from the runtime config (`sessionEncryptionKey`, `NUXT_SESSION_ENCRYPTION_KEY`),
 * so a reader of the Redis data (a backup, a stray client, a shared instance) cannot use them.
 * The stored form is `v1.<base64url(iv | tag | ciphertext)>`, with a fresh random IV per write.
 *
 * @module server/utils/session-crypto
 */

import { createCipheriv, createDecipheriv, randomBytes } from 'node:crypto'

/** Length of the key in bytes (AES-256). */
const KEY_BYTES = 32

/** Length of the GCM initialisation vector in bytes. */
const IV_BYTES = 12

/** Length of the GCM authentication tag in bytes. */
const TAG_BYTES = 16

/** The cipher in use. */
const ALGORITHM = 'aes-256-gcm'

/** Marks the stored form, so a later change of scheme can be told apart. */
const VERSION_PREFIX = 'v1.'

/**
 * Reads the session encryption key from its configured form.
 *
 * @param raw - The configured key: 32 random bytes, base64-encoded; empty when none is set.
 * @returns The key, or null when none is configured and the process runs in development.
 * @throws Error when the key is missing outside development, or is not 32 bytes of base64.
 */
export const readSessionKey = (raw: unknown): Buffer | null => {
  const text = typeof raw === 'string' ? raw.trim() : ''
  if (!text) {
    if (process.env.NODE_ENV === 'development') {
      return null
    }
    throw new Error('The session encryption key (NUXT_SESSION_ENCRYPTION_KEY) is required when sessions are kept in Redis.')
  }
  const key = Buffer.from(text, 'base64')
  if (key.length !== KEY_BYTES) {
    throw new Error('The session encryption key must be 32 random bytes, base64-encoded.')
  }
  return key
}

/**
 * Encrypts a serialised session.
 *
 * @param plain - The session JSON.
 * @param key - The 32-byte key.
 * @returns The stored form.
 */
export const encryptSession = (plain: string, key: Buffer): string => {
  const iv = randomBytes(IV_BYTES)
  const cipher = createCipheriv(ALGORITHM, key, iv)
  const sealed = Buffer.concat([cipher.update(plain, 'utf8'), cipher.final()])
  return `${VERSION_PREFIX}${Buffer.concat([iv, cipher.getAuthTag(), sealed]).toString('base64url')}`
}

/**
 * Decrypts a stored session.
 *
 * @param stored - The stored form from `encryptSession`.
 * @param key - The 32-byte key.
 * @returns The session JSON, or null when the value is not in the stored form, was tampered with,
 * or was written under another key.
 */
export const decryptSession = (stored: string, key: Buffer): string | null => {
  if (!stored.startsWith(VERSION_PREFIX)) {
    return null
  }
  const bytes = Buffer.from(stored.slice(VERSION_PREFIX.length), 'base64url')
  if (bytes.length <= IV_BYTES + TAG_BYTES) {
    return null
  }
  try {
    const decipher = createDecipheriv(ALGORITHM, key, bytes.subarray(0, IV_BYTES))
    decipher.setAuthTag(bytes.subarray(IV_BYTES, IV_BYTES + TAG_BYTES))
    return Buffer.concat([decipher.update(bytes.subarray(IV_BYTES + TAG_BYTES)), decipher.final()]).toString('utf8')
  } catch {
    return null
  }
}

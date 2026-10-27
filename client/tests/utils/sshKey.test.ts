/**
 * Unit tests for the SSH public key pre-checks.
 */
import { describe, expect, it } from 'vitest'
import { isEd25519PublicKey, looksLikePrivateKey, suggestedLabel } from '~/utils/sshKey'

/** A throwaway key generated with ssh-keygen; its comment is `user@host`. */
const KEY = 'ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIG6EJIHRxsxge2aZnl+clPBfoX0nmvD7FfHXM2QQ3+3b user@host'

describe('isEd25519PublicKey', () => {
  it('accepts a real key, with or without comment and whitespace', () => {
    expect(isEd25519PublicKey(KEY)).toBe(true)
    expect(isEd25519PublicKey(`  ${KEY.split(' ').slice(0, 2).join(' ')}\n`)).toBe(true)
  })

  it('refuses other algorithms, truncation and multiple lines', () => {
    expect(isEd25519PublicKey('ssh-rsa AAAAB3NzaC1yc2EAAAADAQABAAABAQ user')).toBe(false)
    expect(isEd25519PublicKey(KEY.slice(0, 60))).toBe(false)
    expect(isEd25519PublicKey(`${KEY}\n${KEY}`)).toBe(false)
  })
})

describe('looksLikePrivateKey', () => {
  it('flags OpenSSH and PEM private keys', () => {
    expect(looksLikePrivateKey('-----BEGIN OPENSSH PRIVATE KEY-----\nabc')).toBe(true)
    expect(looksLikePrivateKey('-----BEGIN RSA PRIVATE KEY-----')).toBe(true)
    expect(looksLikePrivateKey(KEY)).toBe(false)
  })
})

describe('suggestedLabel', () => {
  it('uses the comment', () => {
    expect(suggestedLabel(KEY)).toBe('user@host')
    expect(suggestedLabel('ssh-ed25519 AAAA')).toBe('')
  })
})

/**
 * Tests for `utils/hostingSetup.ts`: what each check accepts, rejects and leaves alone.
 */
import { describe, expect, it } from 'vitest'
import { domainErrorKey, passwordErrorKey, sanitizeUsername, usernameErrorKey } from '~/utils/hostingSetup'

describe('sanitizeUsername', () => {
  it('lowercases and drops everything but letters and digits', () => {
    expect(sanitizeUsername('Web-Master_9!')).toBe('webmaster9')
  })
})

describe('usernameErrorKey', () => {
  it.each([
    ['', ''],
    ['web', ''],
    ['webmast9', ''],
    ['9web', 'client.setup.usernameFormat'],
    ['Web', 'client.setup.usernameFormat'],
    ['we', 'client.setup.usernameMin'],
    ['webmaster1', 'client.setup.usernameMax'],
  ])('reads %s as %s', (username, expected) => {
    expect(usernameErrorKey(username)).toBe(expected)
  })
})

describe('domainErrorKey', () => {
  it.each([
    ['', ''],
    ['   ', ''],
    ['example.com', ''],
    ['sub.example.co.uk', ''],
    ['EXAMPLE.COM', ''],
    ['notadomain', 'client.setup.domainInvalid'],
    ['-bad.com', 'client.setup.domainInvalid'],
    ['example.c', 'client.setup.domainInvalid'],
  ])('reads %s as %s', (domain, expected) => {
    expect(domainErrorKey(domain)).toBe(expected)
  })
})

describe('passwordErrorKey', () => {
  it.each([
    ['', ''],
    ['12345678', ''],
    ['1234567', 'client.setup.passwordMin'],
  ])('reads %s as %s', (password, expected) => {
    expect(passwordErrorKey(password)).toBe(expected)
  })
})

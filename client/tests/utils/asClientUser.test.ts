/**
 * Unit tests for asClientUser: an account record is kept, a sign-in acknowledgement is not.
 */
import { describe, expect, it } from 'vitest'
import { asClientUser } from '~/utils/asClientUser'

describe('asClientUser', () => {
  it('returns an account record untouched', () => {
    const record = { id: 3, firstname: 'Ann', lastname: 'Lee', email: 'ann@acme.test' }

    expect(asClientUser(record)).toBe(record)
  })

  it('returns null for the { success: true } acknowledgement the sign-in endpoints answer with', () => {
    expect(asClientUser({ success: true })).toBeNull()
  })

  it.each([[null], [undefined], ['ok'], [1]])('returns null for %s', (answer) => {
    expect(asClientUser(answer)).toBeNull()
  })
})

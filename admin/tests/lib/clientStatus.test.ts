/**
 * Tests for `restrictsAccount`: only Suspended and Closed restrict the account, so only they are
 * confirmed and spelled out.
 */

import { describe, expect, it } from 'vitest'
import { restrictsAccount } from '@/lib/clientStatus'
import { ClientStatus } from '@/types/client'

describe('restrictsAccount', () => {
  it.each([ClientStatus.Suspended, ClientStatus.Closed])('is true for %s', (status) => {
    expect(restrictsAccount(status)).toBe(true)
  })

  it.each([ClientStatus.Active, ClientStatus.Inactive, ''])('is false for "%s"', (status) => {
    expect(restrictsAccount(status)).toBe(false)
  })
})

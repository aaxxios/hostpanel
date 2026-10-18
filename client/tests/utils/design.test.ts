/**
 * Unit tests for `utils/design.ts`: the avatar initials helper.
 *
 * @module tests/utils/design.test
 */

import { describe, expect, it } from 'vitest'
import { getInitials } from '~/utils/design'

describe('getInitials', () => {
  it('takes the first letter of each word, upper-cased', () => {
    expect(getInitials('ada lovelace')).toBe('AL')
  })

  it('returns a single letter for a single name', () => {
    expect(getInitials('Ada')).toBe('A')
  })
})

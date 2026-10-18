/**
 * Unit tests for `constants/payMethod.ts`.
 *
 * @module tests/constants/payMethod.test
 */

import { describe, expect, it } from 'vitest'
import { NEW_CARD } from '~/constants/payMethod'

describe('NEW_CARD', () => {
  it('is the marker for a card that is not saved yet', () => {
    expect(NEW_CARD).toBe('new')
  })
})

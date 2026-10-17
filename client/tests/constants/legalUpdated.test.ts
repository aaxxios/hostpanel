/**
 * Unit tests for `constants/legalUpdated.ts`.
 *
 * @module tests/constants/legalUpdated.test
 */

import { describe, expect, it } from 'vitest'
import { LEGAL_LAST_UPDATED } from '~/constants/legalUpdated'

describe('LEGAL_LAST_UPDATED', () => {
  it('is a real ISO calendar date', () => {
    expect(new Date(LEGAL_LAST_UPDATED).toISOString().startsWith(LEGAL_LAST_UPDATED)).toBe(true)
  })
})

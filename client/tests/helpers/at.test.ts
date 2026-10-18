/**
 * Tests for `tests/helpers/at.ts`.
 *
 * @module tests/helpers/at.test
 */

import { describe, expect, it } from 'vitest'
import { at } from './at'

describe('at', () => {
  it('returns the item at the index', () => {
    expect(at(['a', 'b'], 1)).toBe('b')
  })

  it('throws when the list is shorter than the index', () => {
    expect(() => at(['a'], 3)).toThrow('Expected an item at index 3, but the list has 1.')
  })
})

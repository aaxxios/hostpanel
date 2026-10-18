/**
 * Unit tests for `constants/catalogGroup.ts`.
 *
 * @module tests/constants/catalogGroup.test
 */

import { describe, expect, it } from 'vitest'
import { HOSTING_GROUP_ID } from '~/constants/catalogGroup'

describe('HOSTING_GROUP_ID', () => {
  it('is a positive product group id', () => {
    expect(Number.isInteger(HOSTING_GROUP_ID)).toBe(true)
    expect(HOSTING_GROUP_ID).toBeGreaterThan(0)
  })
})

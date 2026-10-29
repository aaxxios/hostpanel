/**
 * Tests for `utils/queryId`: only one positive whole number is accepted from a query value.
 *
 * @module tests/utils/queryId.test
 */

import { describe, expect, it } from 'vitest'
import { queryId } from '~/utils/queryId'

describe('queryId', () => {
  it('accepts a positive whole number written as a string or given as a number', () => {
    expect(queryId('42')).toBe('42')
    expect(queryId(42)).toBe('42')
  })

  it('refuses a repeated key', () => {
    expect(queryId(['1', '2'])).toBeUndefined()
    expect(queryId(['1'])).toBeUndefined()
  })

  it('refuses a missing, empty or null value', () => {
    expect(queryId(undefined)).toBeUndefined()
    expect(queryId(null)).toBeUndefined()
    expect(queryId('')).toBeUndefined()
  })

  it('refuses anything that is not a positive whole number', () => {
    expect(queryId('0')).toBeUndefined()
    expect(queryId('-3')).toBeUndefined()
    expect(queryId('1.5')).toBeUndefined()
    expect(queryId('07')).toBeUndefined()
    expect(queryId('../x')).toBeUndefined()
    expect(queryId('12abc')).toBeUndefined()
    expect(queryId(1.5)).toBeUndefined()
    expect(queryId(Number.NaN)).toBeUndefined()
    expect(queryId({})).toBeUndefined()
  })
})

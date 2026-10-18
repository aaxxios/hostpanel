/**
 * Tests for `utils/cardFormat.ts`: digit grouping, the expiry slash and the security code.
 *
 * @module tests/utils/cardFormat.test
 */

import { describe, expect, it } from 'vitest'
import { formatCardNumber, formatCvv, formatExpiry } from '~/utils/cardFormat'

describe('formatCardNumber', () => {
  it('groups digits in fours', () => {
    expect(formatCardNumber('4242424242424242')).toBe('4242 4242 4242 4242')
  })

  it('drops everything that is not a digit', () => {
    expect(formatCardNumber('42a4-2 4')).toBe('4242 4')
  })

  it('keeps at most sixteen digits', () => {
    expect(formatCardNumber('42424242424242429999')).toBe('4242 4242 4242 4242')
  })

  it('returns an empty string for empty input', () => {
    expect(formatCardNumber('')).toBe('')
  })
})

describe('formatExpiry', () => {
  it('leaves one or two digits alone', () => {
    expect(formatExpiry('1')).toBe('1')
    expect(formatExpiry('12')).toBe('12')
  })

  it('inserts the slash after the month', () => {
    expect(formatExpiry('1228')).toBe('12 / 28')
    expect(formatExpiry('123')).toBe('12 / 3')
  })

  it('keeps at most four digits and ignores letters', () => {
    expect(formatExpiry('12 / 28 99')).toBe('12 / 28')
    expect(formatExpiry('ab')).toBe('')
  })
})

describe('formatCvv', () => {
  it('keeps at most four digits', () => {
    expect(formatCvv('12345')).toBe('1234')
  })

  it('drops everything that is not a digit', () => {
    expect(formatCvv('1a2')).toBe('12')
  })
})

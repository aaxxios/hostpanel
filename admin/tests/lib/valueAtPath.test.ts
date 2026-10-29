/**
 * Tests for reading a value out of a nested object by a dot path.
 */
import { describe, expect, it } from 'vitest'
import { valueAtPath } from '@/lib/valueAtPath'

describe('valueAtPath', () => {
  it('reads a top-level property by its name', () => {
    expect(valueAtPath({ status: 'Paid' }, 'status')).toBe('Paid')
  })

  it('reads a property of a nested record by a dot path', () => {
    expect(valueAtPath({ dates: { created: '2026-10-01' } }, 'dates.created')).toBe('2026-10-01')
  })

  it('keeps a null value as null', () => {
    expect(valueAtPath({ dates: { paid: null } }, 'dates.paid')).toBeNull()
  })

  it('answers undefined when a segment on the way is missing', () => {
    expect(valueAtPath({ dates: null }, 'dates.paid')).toBeUndefined()
    expect(valueAtPath({}, 'dates.paid')).toBeUndefined()
  })
})

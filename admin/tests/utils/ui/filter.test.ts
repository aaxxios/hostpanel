/**
 * Unit tests for the advanced-filter vocabulary: the strings a filter row carries.
 */
import { describe, expect, it } from 'vitest'
import { UiFilterCondition } from '@/utils/ui/filter'

describe('UiFilterCondition', () => {
  it('keeps the exact and contains strings', () => {
    expect(Object.values(UiFilterCondition)).toEqual(['exact', 'contains'])
  })
})

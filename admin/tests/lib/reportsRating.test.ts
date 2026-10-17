/**
 * Tests for the tone of a support rating.
 */
import { describe, expect, it } from 'vitest'
import { ratingTone } from '@/lib/reportsRating'
import { UiTone } from '@/utils/ui/tone'

describe('ratingTone', () => {
  it('is success from 8, warning from 5 and danger below', () => {
    expect(ratingTone(10)).toBe(UiTone.Success)
    expect(ratingTone(8)).toBe(UiTone.Success)
    expect(ratingTone(7)).toBe(UiTone.Warning)
    expect(ratingTone(5)).toBe(UiTone.Warning)
    expect(ratingTone(4)).toBe(UiTone.Danger)
  })
})

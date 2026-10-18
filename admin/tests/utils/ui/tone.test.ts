/**
 * Unit tests for the tone vocabulary: every tone has a class in every map.
 */
import { describe, expect, it } from 'vitest'
import {
  toneAlertClasses,
  toneBadgeClasses,
  toneDotClasses,
  toneSolidClasses,
  toneTextClasses,
  UiTone,
} from '@/utils/ui/tone'

describe('tone maps', () => {
  const tones = Object.values(UiTone)

  it.each([
    ['badge', toneBadgeClasses],
    ['alert', toneAlertClasses],
    ['text', toneTextClasses],
    ['solid', toneSolidClasses],
    ['dot', toneDotClasses],
  ])('the %s map covers every tone with non-empty classes', (_name, map) => {
    for (const tone of tones) {
      expect(map[tone].length).toBeGreaterThan(0)
    }
  })

  it('exposes the named members as their literal values', () => {
    expect(UiTone.Danger).toBe('danger')
    expect(UiTone.Primary).toBe('primary')
  })
})

/**
 * Unit tests for `utils/reveal`: the stagger class of the CSS entrance reveal.
 */
import { describe, expect, it } from 'vitest'
import { revealDelay } from '~/utils/reveal'

describe('revealDelay', () => {
  it('delays the animation by the given milliseconds', () => {
    expect(revealDelay(150)).toBe('[animation-delay:150ms]')
  })

  it('starts at once for zero', () => {
    expect(revealDelay(0)).toBe('[animation-delay:0ms]')
  })

  it('never goes negative', () => {
    expect(revealDelay(-40)).toBe('[animation-delay:0ms]')
  })

  it('snaps to the nearest 50 ms step', () => {
    expect(revealDelay(120)).toBe('[animation-delay:100ms]')
  })

  it('caps very long delays', () => {
    expect(revealDelay(99999)).toBe('[animation-delay:1500ms]')
  })
})

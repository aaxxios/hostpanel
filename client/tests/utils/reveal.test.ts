/**
 * Unit tests for `utils/reveal`: the stagger style of the CSS entrance reveal.
 */
import { describe, expect, it } from 'vitest'
import { revealStyle } from '~/utils/reveal'

describe('revealStyle', () => {
  it('delays the animation by the given milliseconds', () => {
    expect(revealStyle(150)).toEqual({ animationDelay: '150ms' })
  })

  it('starts at once for zero', () => {
    expect(revealStyle(0)).toEqual({ animationDelay: '0ms' })
  })

  it('never goes negative', () => {
    expect(revealStyle(-40)).toEqual({ animationDelay: '0ms' })
  })
})

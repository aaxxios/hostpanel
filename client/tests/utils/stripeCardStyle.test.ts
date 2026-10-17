/**
 * Unit tests for the card element's colour-mode style.
 */
import { describe, expect, it } from 'vitest'
import { buildStripeCardStyle } from '~/utils/stripeCardStyle'

describe('buildStripeCardStyle', () => {
  it('uses light text on the dark surface', () => {
    const style = buildStripeCardStyle(true)

    expect(style.base.color).toBe('#ffffff')
    expect(style.invalid.color).toBe('#f87171')
    expect(style.invalid.iconColor).toBe(style.invalid.color)
  })

  it('uses dark text on the light surface', () => {
    const style = buildStripeCardStyle(false)

    expect(style.base.color).toBe('#111827')
    expect(style.invalid.color).toBe('#dc2626')
    expect(style.invalid.iconColor).toBe(style.invalid.color)
  })

  it('keeps the placeholder, font and size the same in both modes', () => {
    const dark = buildStripeCardStyle(true)
    const light = buildStripeCardStyle(false)

    expect(dark.base['::placeholder']).toEqual(light.base['::placeholder'])
    expect(dark.base.fontFamily).toBe(light.base.fontFamily)
    expect(dark.base.fontSize).toBe('16px')
  })
})

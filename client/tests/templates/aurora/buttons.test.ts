/**
 * Unit tests for the aurora button class constants: the bare-button palettes are complete
 * literals made only of template tokens, never hex colours.
 *
 * @module tests/templates/aurora/buttons.test
 */
import { describe, expect, it } from 'vitest'
import { BRAND_BUTTON, LINE_BUTTON, PILL_ACTIVE, PILL_IDLE } from '~/templates/aurora/buttons'

/** Every constant under test. */
const ALL = [BRAND_BUTTON, LINE_BUTTON, PILL_ACTIVE, PILL_IDLE]

describe('aurora button classes', () => {
  it('paints the brand button with the gradient token and the brand tint text', () => {
    expect(BRAND_BUTTON.split(' ')).toEqual(expect.arrayContaining(['bg-brand', 'text-on-tint']))
  })

  it('frames the line button with the template line colour and lights it up on hover', () => {
    expect(LINE_BUTTON.split(' ')).toEqual(expect.arrayContaining(['border-line2', 'hover:border-ac1']))
  })

  it('shares the brand look between the brand button and the active pill', () => {
    expect(PILL_ACTIVE.split(' ')).toEqual(expect.arrayContaining(['bg-brand', 'text-on-tint', 'rounded-full']))
    expect(PILL_IDLE.split(' ')).toContain('text-mut')
  })

  it('contains no hex colour and no interpolation placeholder', () => {
    for (const classes of ALL) {
      expect(classes).not.toMatch(/#[0-9a-fA-F]{3,8}/)
      expect(classes).not.toContain('${')
    }
  })
})

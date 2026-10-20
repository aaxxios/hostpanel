/**
 * Unit tests for the aurora button class constants: the bare-button palettes are complete
 * literals made only of template tokens, never hex colours.
 *
 * @module tests/templates/aurora/buttons.test
 */
import { describe, expect, it } from 'vitest'
import { BRAND_BUTTON, LINE_BUTTON } from '~/templates/aurora/buttons'

/** Every constant under test. */
const ALL = [BRAND_BUTTON, LINE_BUTTON]

describe('aurora button classes', () => {
  it('paints the brand button with the gradient token and the brand tint text', () => {
    expect(BRAND_BUTTON.split(' ')).toEqual(expect.arrayContaining(['bg-brand', 'text-on-brand']))
  })

  it('frames the line button with the template line colour and lights it up on hover', () => {
    expect(LINE_BUTTON.split(' ')).toEqual(expect.arrayContaining(['border-line2', 'hover:border-ac1']))
  })

  it('contains no hex colour and no interpolation placeholder', () => {
    for (const classes of ALL) {
      expect(classes).not.toMatch(/#[0-9a-fA-F]{3,8}/)
      expect(classes).not.toContain('${')
    }
  })
})

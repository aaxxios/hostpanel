/**
 * Tests for the palette module the admin keeps as a copy of the storefront's: colour parsing, the
 * eleven-step scale pinned at the picked colour, readable text colour and the CSS declarations.
 */
import { describe, expect, it } from 'vitest'
import {
  SHADE_STEPS,
  bestTextOn,
  buildScale,
  contrastRatio,
  hexToRgb,
  rgbToHex,
  scaleToDeclarations,
} from '@/lib/brandPalette'

describe('hexToRgb and rgbToHex', () => {
  it('parse a six-digit colour in any case and round-trip it', () => {
    expect(hexToRgb('#FF8000')).toEqual([255, 128, 0])
    expect(rgbToHex([255, 128, 0])).toBe('#ff8000')
  })

  it('refuse anything that is not a six-digit hex colour', () => {
    expect(hexToRgb('red')).toBeNull()
    expect(hexToRgb('#fff')).toBeNull()
  })
})

describe('buildScale', () => {
  it('has every step and pins 500 at the picked colour', () => {
    const scale = buildScale('#3366cc')

    expect(Object.keys(scale).map(Number)).toEqual([...SHADE_STEPS])
    expect(rgbToHex(scale[500])).toBe('#3366cc')
  })

  it('goes from light to dark', () => {
    const scale = buildScale('#3366cc')

    expect(contrastRatio(rgbToHex(scale[50]), '#ffffff')).toBeLessThan(contrastRatio(rgbToHex(scale[950]), '#ffffff'))
  })
})

describe('bestTextOn', () => {
  it('picks white on a dark colour and near-black on a light one', () => {
    expect(bestTextOn('#101010')).toBe('#ffffff')
    expect(bestTextOn('#f5f5f5')).toBe('#111111')
  })
})

describe('scaleToDeclarations', () => {
  it('writes one custom property per step', () => {
    const css = scaleToDeclarations('primary', buildScale('#3366cc'))

    expect(css).toContain('--primary-500:')
    expect(css).toContain('--primary-950:')
  })
})

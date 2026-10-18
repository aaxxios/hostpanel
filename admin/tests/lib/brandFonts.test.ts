/**
 * Tests for the brand font options: every face has a label, a literal preview class and the
 * values the API accepts.
 */
import { describe, expect, it } from 'vitest'
import { BRAND_FONT_OPTIONS, BrandFont, previewClass } from '@/lib/brandFonts'

describe('BRAND_FONT_OPTIONS', () => {
  it('lists the four faces the API accepts, in order', () => {
    expect(BRAND_FONT_OPTIONS.map((option) => option.value)).toEqual([
      'inter',
      'noto-sans-armenian',
      'noto-serif-armenian',
      'system',
    ])
  })

  it('gives every face a label key and a font-brand utility class', () => {
    for (const option of BRAND_FONT_OPTIONS) {
      expect(option.labelKey).toMatch(/^settings\.typography\.fonts\./)
      expect(option.fontClass).toBe(`font-brand-${option.value}`)
    }
  })
})

describe('previewClass', () => {
  it('returns the utility class of a known face', () => {
    expect(previewClass(BrandFont.NotoSerifArmenian)).toBe('font-brand-noto-serif-armenian')
  })

  it('returns an empty class for the template default and for an unknown value', () => {
    expect(previewClass('')).toBe('')
    expect(previewClass('comic-sans')).toBe('')
  })
})

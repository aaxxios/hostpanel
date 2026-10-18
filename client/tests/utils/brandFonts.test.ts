/**
 * Unit tests for `utils/brandFonts.ts`: every face has a stack and unknown values resolve to none.
 *
 * @module tests/utils/brandFonts.test
 */

import { describe, expect, it } from 'vitest'
import { BRAND_FONTS, BrandFont, FONT_STACKS, fontStack } from '~/utils/brandFonts'

describe('BRAND_FONTS', () => {
  it('lists every face in declaration order', () => {
    expect(BRAND_FONTS).toEqual([BrandFont.Inter, BrandFont.NotoSansArmenian, BrandFont.NotoSerifArmenian, BrandFont.System])
  })

  it('has a stack for every face, each reaching an Armenian-capable face', () => {
    for (const font of BRAND_FONTS) {
      expect(FONT_STACKS[font]).toContain('Armenian')
    }
  })
})

describe('fontStack', () => {
  it('resolves a known name to its stack', () => {
    expect(fontStack('inter')).toBe(FONT_STACKS[BrandFont.Inter])
  })

  it.each([['unknown'], [''], [null], [undefined]])('returns an empty string for %s', (value) => {
    expect(fontStack(value)).toBe('')
  })
})

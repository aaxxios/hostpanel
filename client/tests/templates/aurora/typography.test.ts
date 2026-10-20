/**
 * Unit tests for the aurora typography class constants: the heading strings are complete literals
 * made only of template tokens, never hex colours.
 *
 * @module tests/templates/aurora/typography.test
 */
import { describe, expect, it } from 'vitest'
import {
  BLOCK_TITLE_CLASS,
  EYEBROW_CLASS,
  PAGE_TITLE_CLASS,
  SECTION_SPACING_CLASS,
  SECTION_TITLE_CLASS,
} from '~/templates/aurora/typography'

/** Every constant under test. */
const ALL = [EYEBROW_CLASS, SECTION_TITLE_CLASS, BLOCK_TITLE_CLASS, PAGE_TITLE_CLASS, SECTION_SPACING_CLASS]

describe('aurora typography classes', () => {
  it('uses the aurora text tokens for headings', () => {
    for (const title of [SECTION_TITLE_CLASS, BLOCK_TITLE_CLASS, PAGE_TITLE_CLASS]) {
      expect(title.split(' ')).toEqual(expect.arrayContaining(['font-display', 'font-bold', 'text-tx']))
    }
  })

  it('contains no hex colour and no interpolation placeholder', () => {
    for (const classes of ALL) {
      expect(classes).not.toMatch(/#[0-9a-fA-F]{3,8}/)
      expect(classes).not.toContain('${')
    }
  })

  it('steps the headings down from page to section to block, with a tight leading', () => {
    expect(PAGE_TITLE_CLASS).toContain('clamp(30px,4.2vw,46px)')
    expect(SECTION_TITLE_CLASS).toContain('clamp(26px,3.2vw,36px)')
    expect(BLOCK_TITLE_CLASS).toContain('clamp(24px,2.8vw,32px)')
    for (const title of [SECTION_TITLE_CLASS, BLOCK_TITLE_CLASS, PAGE_TITLE_CLASS]) {
      expect(title.split(' ')).toContain('leading-[1.15]')
    }
  })
})

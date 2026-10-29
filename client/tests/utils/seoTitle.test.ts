/**
 * Unit tests for the detail-page title and description helpers.
 *
 * @module tests/utils/seoTitle.test
 */
import { describe, expect, it } from 'vitest'
import { detailTitle, plainDescription } from '~/utils/seoTitle'

describe('detailTitle', () => {
  it('joins the item and the section with a plain separator and no dash', () => {
    const title = detailTitle('Point a domain', 'Knowledge base')

    expect(title).toBe('Point a domain | Knowledge base')
    expect(title).not.toMatch(/[–—]/)
  })
})

describe('plainDescription', () => {
  it('returns the text of the body without markup or scripts', () => {
    expect(plainDescription('<p>We will <strong>restart</strong> servers.</p><script>alert(1)</script>')).toBe('We will restart servers.')
  })

  it('collapses whitespace between blocks', () => {
    expect(plainDescription('<p>One</p>\n\n<p>  Two  </p>')).toBe('One Two')
  })

  it('cuts a long text at a word boundary and marks the cut', () => {
    const result = plainDescription('<p>alpha beta gamma delta</p>', 14)

    expect(result).toBe('alpha beta…')
    expect(result.length).toBeLessThanOrEqual(14)
  })

  it('keeps a text that fits as it is and gives nothing for an empty body', () => {
    expect(plainDescription('<p>short</p>', 14)).toBe('short')
    expect(plainDescription('')).toBe('')
  })
})

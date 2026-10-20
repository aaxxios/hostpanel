/**
 * Unit tests for the TLD category label helpers: known categories translate, unknown ones are
 * capitalised, blanks are empty.
 *
 * @module tests/utils/tldCategory.test
 */

import { describe, expect, it } from 'vitest'
import { TldCategory } from '~/types/tldcategory'
import { tldCategoryLabel, tldCategorySlug } from '~/utils/tldCategory'

/** Translator that echoes the key so the lookup is visible. */
const echo = (key: string): string => `T(${key})`

describe('tldCategorySlug', () => {
  it('trims, lower-cases and dashes spaces', () => {
    expect(tldCategorySlug('  Real Estate ')).toBe('real-estate')
  })
})

describe('tldCategoryLabel', () => {
  it.each(Object.values(TldCategory))('translates the known category %s', (category) => {
    expect(tldCategoryLabel(category, echo)).toBe(`T(domains.pricing.categories.${category})`)
  })

  it('matches a registrar spelling regardless of case', () => {
    expect(tldCategoryLabel('Budget', echo)).toBe('T(domains.pricing.categories.budget)')
  })

  it('capitalises an unknown category instead of showing the raw slug', () => {
    expect(tldCategoryLabel('gaming', echo)).toBe('Gaming')
    expect(tldCategoryLabel('eco-friendly', echo)).toBe('Eco friendly')
  })

  it('answers an empty string for a blank name', () => {
    expect(tldCategoryLabel('  ', echo)).toBe('')
  })
})

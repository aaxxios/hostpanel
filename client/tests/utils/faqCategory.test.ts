/**
 * Unit tests for the FAQ topic slug helpers: enum to URL slug and back.
 *
 * @module tests/utils/faqCategory.test
 */

import { describe, expect, it } from 'vitest'
import { faqCategoryFromParam, faqCategoryFromSlug, faqCategorySlug } from '~/utils/faqCategory'
import { FaqCategory } from '~/types/faqcategory'

describe('faqCategorySlug', () => {
  it('lower-cases the enum value', () => {
    expect(faqCategorySlug(FaqCategory.General)).toBe('general')
    expect(faqCategorySlug(FaqCategory.Products)).toBe('products')
  })
})

describe('faqCategoryFromSlug', () => {
  it.each(Object.values(FaqCategory))('maps the slug of %s back to the topic', (category) => {
    expect(faqCategoryFromSlug(faqCategorySlug(category))).toBe(category)
  })

  it('answers undefined for a slug that names no topic', () => {
    expect(faqCategoryFromSlug('nope')).toBeUndefined()
    expect(faqCategoryFromSlug('General')).toBeUndefined()
  })
})

describe('faqCategoryFromParam', () => {
  it('reads a string parameter', () => {
    expect(faqCategoryFromParam('products')).toBe(FaqCategory.Products)
  })

  it('reads the first entry of an array parameter', () => {
    expect(faqCategoryFromParam(['products', 'general'])).toBe(FaqCategory.Products)
  })

  it('answers undefined for an absent, empty or unknown parameter', () => {
    expect(faqCategoryFromParam(undefined)).toBeUndefined()
    expect(faqCategoryFromParam([])).toBeUndefined()
    expect(faqCategoryFromParam('nope')).toBeUndefined()
  })
})

/**
 * Tests for the knowledgebase category slug used in test ids.
 */
import { describe, expect, it } from 'vitest'
import { kbCategorySlug } from '~/utils/kbCategory'

describe('kbCategorySlug', () => {
  it('lowercases a single word', () => {
    expect(kbCategorySlug('Billing')).toBe('billing')
  })

  it('turns spaces and punctuation into single dashes', () => {
    expect(kbCategorySlug('Billing & Payments')).toBe('billing-payments')
    expect(kbCategorySlug('Getting   Started')).toBe('getting-started')
  })

  it('drops dashes at either end', () => {
    expect(kbCategorySlug('  (Email) ')).toBe('email')
  })

  it('keeps letters of other scripts', () => {
    expect(kbCategorySlug('Оплата и счета')).toBe('оплата-и-счета')
  })
})

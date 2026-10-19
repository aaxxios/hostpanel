/**
 * Unit tests for `constants/uiDefaults.ts`.
 *
 * @module tests/constants/uiDefaults.test
 */

import { describe, expect, it } from 'vitest'
import { DEFAULT_PAGE_SIZE, RECENTLY_ADDED_MS, SUCCESS_BANNER_MS, TESTIMONIAL_AUTOPLAY_MS } from '~/constants/uiDefaults'

describe('ui defaults', () => {
  it('keeps the banner longer than the cart flag and the page size positive', () => {
    expect(SUCCESS_BANNER_MS).toBeGreaterThan(RECENTLY_ADDED_MS)
    expect(DEFAULT_PAGE_SIZE).toBeGreaterThan(0)
  })

  it('advances the testimonial carousel after a positive delay', () => {
    expect(TESTIMONIAL_AUTOPLAY_MS).toBeGreaterThan(0)
  })
})

/**
 * Unit tests for `utils/ui/embed.ts`: every embed height step has height classes.
 *
 * @module tests/utils/ui/embed.test
 */

import { describe, expect, it } from 'vitest'
import { EMBED_HEIGHT_CLASSES, UiEmbedHeight } from '~/utils/ui/embed'

describe('EMBED_HEIGHT_CLASSES', () => {
  it('has height utilities for every step', () => {
    for (const step of Object.values(UiEmbedHeight)) {
      expect(EMBED_HEIGHT_CLASSES[step].startsWith('h-')).toBe(true)
    }
  })

  it('grows from compact to tall', () => {
    expect(EMBED_HEIGHT_CLASSES[UiEmbedHeight.Sm]).toBe('h-48 md:h-64')
    expect(EMBED_HEIGHT_CLASSES[UiEmbedHeight.Lg]).toBe('h-64 md:h-96')
  })
})

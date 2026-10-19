/**
 * Unit tests for `constants/headerWidget.ts`.
 *
 * @module tests/constants/headerWidget.test
 */

import { describe, expect, it } from 'vitest'
import { HEADER_WIDGET_CACHE_BUST, HEADER_WIDGET_PATH } from '~/constants/headerWidget'

describe('header widget constants', () => {
  it('keeps the script path and the shared cache-bust token', () => {
    expect(HEADER_WIDGET_PATH).toBe('/widget/header.js')
    expect(HEADER_WIDGET_CACHE_BUST).toBe('20260918')
  })
})

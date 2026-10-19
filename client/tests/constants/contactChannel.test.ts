/**
 * Unit tests for `constants/contactChannel.ts`.
 *
 * @module tests/constants/contactChannel.test
 */

import { describe, expect, it } from 'vitest'
import { TELEGRAM_LINK_BASE, WHATSAPP_LINK_BASE } from '~/constants/contactChannel'

describe('contact channel bases', () => {
  it('are secure addresses that end where the handle or number begins', () => {
    expect(TELEGRAM_LINK_BASE).toBe('https://t.me/')
    expect(WHATSAPP_LINK_BASE).toBe('https://wa.me/')
  })
})

/**
 * Unit tests for `constants/contactInfo.ts`: the display and link forms agree.
 *
 * @module tests/constants/contactInfo.test
 */

import { describe, expect, it } from 'vitest'
import {
  CONTACT_EMAIL,
  CONTACT_PHONE_DISPLAY,
  CONTACT_PHONE_LINK,
  CONTACT_TELEGRAM_URL,
  CONTACT_WHATSAPP_URL,
  OFFICE_LATITUDE,
  OFFICE_LONGITUDE,
} from '~/constants/contactInfo'

describe('contact info', () => {
  it('has an e-mail address', () => {
    expect(CONTACT_EMAIL).toContain('@')
  })

  it('shows the phone with spaces and links it as bare digits', () => {
    expect(CONTACT_PHONE_DISPLAY.split(' ').join('')).toBe(CONTACT_PHONE_LINK)
    expect(CONTACT_PHONE_LINK.startsWith('+')).toBe(true)
  })

  it('points the chat links at https', () => {
    expect(CONTACT_TELEGRAM_URL.startsWith('https://')).toBe(true)
    expect(CONTACT_WHATSAPP_URL.startsWith('https://')).toBe(true)
  })

  it('keeps the office coordinates in range', () => {
    expect(Math.abs(OFFICE_LATITUDE)).toBeLessThanOrEqual(90)
    expect(Math.abs(OFFICE_LONGITUDE)).toBeLessThanOrEqual(180)
  })
})

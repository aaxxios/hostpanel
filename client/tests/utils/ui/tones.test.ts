/**
 * Unit tests for the kit's class maps: every enum member has an entry, and the field state helper
 * switches between the error, open, idle and disabled looks.
 */
import { describe, expect, it } from 'vitest'
import { UiAlertVariant, UiSize, UiTone } from '~/utils/ui/enums'
import {
  ALERT_TONE_CLASSES,
  BADGE_TONE_CLASSES,
  BUTTON_SIZE_CLASSES,
  CONTROL_SIZE_CLASSES,
  ICON_TONE_CLASSES,
  fieldStateClasses,
} from '~/utils/ui/tones'

describe('tone maps', () => {
  it.each(Object.values(UiTone))('has badge and icon classes for the %s tone', (tone) => {
    expect(BADGE_TONE_CLASSES[tone]).toBeTruthy()
    expect(ICON_TONE_CLASSES[tone]).toBeTruthy()
  })

  it.each(Object.values(UiAlertVariant))('has all four alert parts for the %s variant', (variant) => {
    const parts = ALERT_TONE_CLASSES[variant]

    expect(parts.wrap).toBeTruthy()
    expect(parts.icon).toBeTruthy()
    expect(parts.title).toBeTruthy()
    expect(parts.body).toBeTruthy()
  })

  it.each(Object.values(UiSize))('has control and button sizes for %s', (size) => {
    expect(CONTROL_SIZE_CLASSES[size]).toBeTruthy()
    expect(BUTTON_SIZE_CLASSES[size]).toBeTruthy()
  })

  it('gives every tone a light and a dark variant', () => {
    Object.values(UiTone).forEach((tone) => {
      expect(BADGE_TONE_CLASSES[tone]).toContain('dark:')
    })
  })
})

describe('fieldStateClasses', () => {
  it('uses the idle look by default', () => {
    const classes = fieldStateClasses({})

    expect(classes).toContain('border-gray-300')
    expect(classes).toContain('focus:ring-primary-500/50')
    expect(classes).not.toContain('opacity-60')
  })

  it('uses the error look', () => {
    const classes = fieldStateClasses({ error: true })

    expect(classes).toContain('border-red-400')
    expect(classes).not.toContain('border-gray-300')
  })

  it('keeps the focus look while a popup is open', () => {
    expect(fieldStateClasses({ open: true })).toContain('ring-2 ring-primary-500/50')
  })

  it('lets the error win over the open look', () => {
    const classes = fieldStateClasses({ error: true, open: true })

    expect(classes).toContain('border-red-400')
    expect(classes).not.toContain('ring-primary-500/50 bg-white')
  })

  it('dims a disabled field', () => {
    const classes = fieldStateClasses({ disabled: true })

    expect(classes).toContain('opacity-60')
    expect(classes).toContain('cursor-not-allowed')
  })
})

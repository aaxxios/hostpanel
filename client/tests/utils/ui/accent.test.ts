/**
 * Unit tests for the accent helpers: which colours may reach a style and that every appearance and
 * tile size has classes.
 */
import { describe, expect, it } from 'vitest'
import {
  ACCENT_BUTTON_CLASSES,
  ACCENT_CLASSES,
  ACCENT_TILE_CLASSES,
  UiAccentAppearance,
  UiAccentTileSize,
  isAccentColor,
} from '~/utils/ui/accent'

describe('isAccentColor', () => {
  it.each(['#fff', '#0ea5e9', '#0EA5E9', '#0ea5e980', '#abcd'])('accepts the hex %s', (value) => {
    expect(isAccentColor(value)).toBe(true)
  })

  it.each(['red', 'url(x)', '#12', '#12345', '#0ea5e9; color: red', 'rgb(0,0,0)', '', undefined, null])(
    'refuses %s',
    (value) => {
      expect(isAccentColor(value as string | null | undefined)).toBe(false)
    },
  )
})

describe('accent class maps', () => {
  it.each(Object.values(UiAccentAppearance))('has button classes for the %s appearance', (appearance) => {
    expect(ACCENT_BUTTON_CLASSES[appearance]).toContain('var(--accent)')
  })

  it.each(Object.values(UiAccentTileSize))('has box and icon classes for the %s tile', (size) => {
    expect(ACCENT_TILE_CLASSES[size].box).toBeTruthy()
    expect(ACCENT_TILE_CLASSES[size].icon).toBeTruthy()
  })

  it('reads the accent variable in every colour class', () => {
    for (const value of Object.values(ACCENT_CLASSES)) {
      expect(value).toContain('var(--accent)')
    }
  })
})

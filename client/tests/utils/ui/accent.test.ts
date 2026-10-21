/**
 * Unit tests for the accent helpers: which colours may reach a style and that every appearance and
 * tile size has classes.
 */
import { describe, expect, it } from 'vitest'
import {
  ACCENT_BRAND_CLASS,
  ACCENT_BUTTON_CLASSES,
  ACCENT_CLASSES,
  ACCENT_TILE_CLASSES,
  UiAccentAppearance,
  UiAccentTileSize,
  accentInk,
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

describe('accentInk', () => {
  it('puts white on a dark accent and near-black on a pale one', () => {
    expect(accentInk('#0369a1')).toBe('white')
    expect(accentInk('#fde047')).toBe('#08090f')
    expect(accentInk('#fff')).toBe('#08090f')
  })
})

describe('accent text contrast', () => {
  it('darkens the accent text on a light page and lightens it on a dark one', () => {
    expect(ACCENT_CLASSES.text).toContain('_65%,black')
    expect(ACCENT_CLASSES.text).toContain('dark:text-[color:color-mix(in_srgb,var(--accent)_75%,white)]')
  })

  it('reads the ink colour on a solid accent button, never a fixed white', () => {
    expect(ACCENT_BUTTON_CLASSES[UiAccentAppearance.Solid]).toContain('var(--accent-ink')
    expect(ACCENT_BUTTON_CLASSES[UiAccentAppearance.Solid]).not.toContain('text-white')
  })
})

describe('ACCENT_BRAND_CLASS', () => {
  it('points the accent variable at the brand primary with a fallback', () => {
    expect(ACCENT_BRAND_CLASS).toBe('[--accent:rgb(var(--brand-primary-500,2_119_182))]')
    expect(ACCENT_BRAND_CLASS).toContain('--accent:')
    expect(ACCENT_BRAND_CLASS).toContain('--brand-primary-500')
  })
})

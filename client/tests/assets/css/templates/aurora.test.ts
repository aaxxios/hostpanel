/**
 * Guards the aurora colour tokens: the brand gradient must be defined per colour mode, and the
 * light mode must be built from the saturated 600 shades so it stays vivid and carries white text.
 */
import { readFileSync } from 'node:fs'
import { resolve } from 'node:path'
import { describe, expect, it } from 'vitest'

/** The aurora stylesheet source. */
const css = readFileSync(resolve(__dirname, '../../../../assets/css/templates/aurora.css'), 'utf8')

/** The declarations of the `:root.light` block. */
const light = css.slice(css.indexOf(':root.light {'))
/** The declarations of the dark `:root` block. */
const dark = css.slice(0, css.indexOf(':root.light {'))

describe('aurora tokens', () => {
  it('defines the brand gradient for dark mode from the light 400 tint', () => {
    expect(dark).toMatch(/--brand-grad:[^;]*brand-primary-400/)
  })

  it('redefines the brand gradient for light mode from the 600 shades', () => {
    expect(light).toMatch(/--brand-grad:[^;]*brand-accent-600[^;]*brand-primary-600/)
  })

  it('defines the call-to-action wash and the trust chip in both modes', () => {
    expect(dark).toContain('--cta-card:')
    expect(light).toContain('--cta-card:')
    expect(dark).toContain('--chip-bg:')
    expect(light).toContain('--chip-bg:')
  })

  it('puts white text on the light gradient', () => {
    expect(light).toContain('--brand-on-tint: 255 255 255')
  })
})

/** The Tailwind configuration source, where the primary ramp's fallback literals live. */
const tailwind = readFileSync(resolve(__dirname, '../../../../tailwind.config.ts'), 'utf8')

/**
 * Reads the fallback literal of a brand variable.
 *
 * @param source - The stylesheet or configuration text.
 * @param name - The variable name without its leading dashes.
 * @returns The first `r g b` fallback written for it, or an empty string.
 */
const fallbackOf = (source: string, name: string): string =>
  new RegExp(`--${name},\s*(\d+ \d+ \d+)`).exec(source)?.[1] ?? ''

describe('aurora fallbacks agree with the Tailwind ramp', () => {
  it.each(['brand-primary-500', 'brand-primary-600', 'brand-accent-700'])('uses one literal for %s', (name) => {
    const expected = fallbackOf(tailwind, name)

    expect(expected).not.toBe('')
    expect(fallbackOf(css, name)).toBe(expected)
  })

  it('keeps white on primary-500 and primary-600 text above 4.5:1 by fallback', () => {
    /** Relative luminance of an `r g b` string. */
    const lum = (rgb: string): number => {
      const [r = 0, g = 0, b = 0] = rgb.split(' ').map((v) => {
        const c = Number(v) / 255
        return c <= 0.04045 ? c / 12.92 : ((c + 0.055) / 1.055) ** 2.4
      })
      return 0.2126 * r + 0.7152 * g + 0.0722 * b
    }
    for (const name of ['brand-primary-500', 'brand-primary-600', 'brand-accent-700']) {
      expect(1.05 / (lum(fallbackOf(tailwind, name)) + 0.05)).toBeGreaterThanOrEqual(4.5)
    }
  })
})

/**
 * Guards the classic template's colour tokens: a complete light set on `:root`, a dark set
 * under `.dark`, and a page body that follows the colour-mode tokens instead of a fixed dark
 * colour (without it classic stays dark whatever the toggle says).
 */
import { readFileSync } from 'node:fs'
import { resolve } from 'node:path'
import { describe, expect, it } from 'vitest'

/** The classic stylesheet source. */
const css = readFileSync(resolve(__dirname, '../../../../assets/css/templates/classic.css'), 'utf8')

/** The global stylesheet source, which paints the body. */
const global = readFileSync(resolve(__dirname, '../../../../assets/styles/global.css'), 'utf8')

/** The declarations of the light `:root` block. */
const light = css.slice(css.indexOf(':root {'), css.indexOf('.dark {'))

/** The declarations of the `.dark` block. */
const dark = css.slice(css.indexOf('.dark {'), css.indexOf('@keyframes'))

/** The tokens a classic page paints with. */
const SURFACE_TOKENS = ['--classic-page', '--classic-surface', '--classic-footer']

/**
 * Reads an `r g b` token out of a CSS block. A brand token is `var(--brand-primary-N, r g b)`,
 * derived from the operator's brand colour; its fallback triplet is the colour checked here.
 *
 * @param block - The `:root` or `.dark` block source.
 * @param name - Token name without the `--classic-` prefix.
 * @returns The three channels.
 */
const channels = (block: string, name: string): number[] =>
  // Non-null: every token is declared in both blocks, and the test fails loudly when one is not.
  new RegExp(`--classic-${name}:\\s*(?:var\\(--brand-primary-\\d+,\\s*)?(\\d+) (\\d+) (\\d+)\\)?;`).exec(block)!.slice(1).map(Number)

/**
 * WCAG relative luminance of an sRGB colour.
 *
 * @param rgb - The three channels, 0 to 255.
 * @returns The luminance.
 */
const luminance = (rgb: number[]): number => {
  const [r, g, b] = rgb.map((channel) => {
    const c = channel / 255
    return c <= 0.03928 ? c / 12.92 : ((c + 0.055) / 1.055) ** 2.4
  }) as [number, number, number]
  return 0.2126 * r + 0.7152 * g + 0.0722 * b
}

/**
 * WCAG contrast ratio between two colours.
 *
 * @param a - First colour.
 * @param b - Second colour.
 * @returns The ratio, 1 to 21.
 */
const contrast = (a: number[], b: number[]): number => {
  const [hi, lo] = [luminance(a), luminance(b)].sort((x, y) => y - x) as [number, number]
  return (hi + 0.05) / (lo + 0.05)
}

describe('classic tokens', () => {
  it('derives the brand from the operator brand primary, with a fallback, in both modes', () => {
    expect(light).toContain('--classic-brand: var(--brand-primary-700,')
    expect(dark).toContain('--classic-brand: var(--brand-primary-400,')
  })

  it('declares every surface token for the light palette on :root', () => {
    for (const token of SURFACE_TOKENS) {
      expect(light).toContain(`${token}:`)
    }
  })

  it('declares every surface token again for the dark palette', () => {
    for (const token of SURFACE_TOKENS) {
      expect(dark).toContain(`${token}:`)
    }
  })

  it('gives the page a different value in each mode', () => {
    /**
     * Page token value of a block.
     *
     * @param block CSS rule body to search.
     * @returns The trimmed `--classic-page` value.
     */
    const value = (block: string): string => /--classic-page:\s*([^;]+);/.exec(block)![1]!.trim()

    expect(value(light)).not.toBe(value(dark))
  })

  it('paints the body from the colour-mode tokens, not a fixed dark colour', () => {
    const body = /(?:^|\n)body \{([^}]*)\}/.exec(global)![1]!

    expect(body).toContain('var(--page)')
    expect(body).toContain('var(--tx)')
    expect(body).not.toContain('#0a0a0f')
  })

  it('keeps AA contrast for ink, muted and brand on page and surface in both modes', () => {
    for (const block of [light, dark]) {
      for (const bg of ['page', 'surface']) {
        for (const fg of ['ink', 'muted', 'brand']) {
          expect(contrast(channels(block, fg), channels(block, bg))).toBeGreaterThanOrEqual(4.5)
        }
      }
      expect(contrast(channels(block, 'brand'), channels(block, 'brand-soft'))).toBeGreaterThanOrEqual(4.5)
    }
  })

  it('keeps white text at AA on every solid fill state (rest, hover, pressed) in both modes', () => {
    for (const block of [light, dark]) {
      expect(channels(block, 'on-brand')).toEqual([255, 255, 255])
      for (const fill of ['fill', 'fill-hover', 'fill-pressed']) {
        expect(contrast(channels(block, 'on-brand'), channels(block, fill))).toBeGreaterThanOrEqual(4.5)
      }
    }
  })

  it('keeps the solid fill and the strong border distinct from the page and surface (3:1 for the border)', () => {
    for (const block of [light, dark]) {
      for (const bg of ['page', 'surface']) {
        expect(contrast(channels(block, 'border-strong'), channels(block, bg))).toBeGreaterThanOrEqual(3)
        expect(contrast(channels(block, 'fill'), channels(block, bg))).toBeGreaterThanOrEqual(3)
      }
    }
  })

  it('uses a deeper fill than the text brand in dark, never the pastel brand step', () => {
    expect(dark).toMatch(/--classic-fill:\s*var\(--brand-primary-600,/)
    expect(luminance(channels(dark, 'fill'))).toBeLessThan(luminance(channels(dark, 'brand')))
  })

  it('paints primary buttons, chips, segments and the active nav entry from the fill token with on-brand text', () => {
    for (const hook of ['.ui-btn.ui-btn-primary', '.ui-chip.ui-chip-active', '.ui-segment.ui-segment-active', '.ui-nav-item.ui-nav-item-active']) {
      const rule = new RegExp(`${hook.replace(/\./g, '\\.')} \\{([^}]*)\\}`).exec(css)
      expect(rule, hook).not.toBeNull()
      expect(rule![1]).toContain('background-color: rgb(var(--classic-fill))')
      expect(rule![1]).toContain('color: rgb(var(--classic-on-brand))')
    }
  })

  it('draws outline and secondary buttons and idle chips with the strong border token', () => {
    expect(css).toMatch(/\.ui-btn\.ui-btn-outline \{[^}]*border: 1px solid rgb\(var\(--classic-border-strong\)\)/)
    expect(css).toMatch(/\.ui-btn\.ui-btn-secondary \{[^}]*border: 1px solid rgb\(var\(--classic-border-strong\)\)/)
    expect(css).toMatch(/\.ui-chip:not\(\.ui-chip-active\) \{[^}]*border-color: rgb\(var\(--classic-border-strong\)\)/)
  })

  it('restyles the kit only under the classic scope, with no gradient or glow', () => {
    const kit = css.slice(css.indexOf('--ui-surface'))
    const selectors = kit.match(/^[^\s@/*][^{]*\{/gm) ?? []

    expect(selectors.length).toBeGreaterThan(10)
    for (const selector of selectors.filter(rule => rule.includes('.ui-'))) {
      expect(selector).toContain(":is(.tpl-classic, html[data-template='classic'])")
    }
    expect(kit).toContain('--brand-grad: linear-gradient(rgb(var(--classic-fill)), rgb(var(--classic-fill)))')
    expect(kit).not.toMatch(/radial-gradient|blur\(/)
  })
})

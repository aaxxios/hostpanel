/**
 * Guards the footer surface tokens of the three templates: in both colour modes the footer is a
 * visibly separate surface from the page, and the footer link colours (resting and hover) keep
 * WCAG AA contrast (4.5:1) on it. Values are read from the stylesheets, so a token edit that
 * breaks either property fails here.
 */
import { readFileSync } from 'node:fs'
import { resolve } from 'node:path'
import { describe, expect, it } from 'vitest'

/**
 * Reads a template stylesheet.
 *
 * @param name Template name, the file name without `.css`.
 * @returns The stylesheet text.
 */
const read = (name: string): string => readFileSync(resolve(__dirname, `../../../../assets/css/templates/${name}.css`), 'utf8')

/** The aurora stylesheet source. */
const aurora = read('aurora')

/** The nova stylesheet source. */
const nova = read('nova')

/** The classic stylesheet source. */
const classic = read('classic')

/** Minimum contrast ratio for normal text (WCAG AA). */
const AA = 4.5

/** Minimum luminance ratio between the dark footer and the dark page. In light mode the footer is white on a near-white page, so the border and shadow carry the separation instead. */
const DARK_SEPARATION = 1.15

/**
 * Takes the text of one rule block: from the first match of `start` to the next closing brace.
 *
 * @param css Stylesheet source.
 * @param start Text that opens the block.
 * @returns The block body.
 */
const block = (css: string, start: string): string => {
  const from = css.indexOf(start)
  return css.slice(from, css.indexOf('}', from))
}

/**
 * Reads a token's colour as r, g, b from a hex value or a space separated triplet.
 *
 * @param source Block source.
 * @param token Token name, e.g. `--footer`.
 * @returns The three channels, 0 to 255.
 */
const channels = (source: string, token: string): [number, number, number] => {
  const match = new RegExp(`${token}:\\s*([^;]+);`).exec(source)
  if (!match) {
    throw new Error(`token ${token} not found`)
  }
  const value = match[1]!.trim()
  const hex = /^#([0-9a-f]{6})$/i.exec(value)
  if (hex) {
    const n = parseInt(hex[1]!, 16)
    return [(n >> 16) & 255, (n >> 8) & 255, n & 255]
  }
  const triplet = value.match(/\d+/g)
  if ((value.startsWith('rgb(') || value.startsWith('var(')) && triplet) {
    // rgb(var(--x, 1 2 3)) and var(--brand-primary-N, 1 2 3) keep their fallback as the last three numbers.
    return triplet.slice(-3).map(Number) as [number, number, number]
  }
  if (triplet && triplet.length === 3) {
    return triplet.map(Number) as [number, number, number]
  }
  throw new Error(`token ${token} has no solid colour: ${value}`)
}

/**
 * WCAG relative luminance of a colour.
 *
 * @param rgb Channels 0 to 255.
 * @returns Luminance 0 to 1.
 */
const luminance = (rgb: [number, number, number]): number => {
  const [r, g, b] = rgb.map((c) => {
    const s = c / 255
    return s <= 0.03928 ? s / 12.92 : ((s + 0.055) / 1.055) ** 2.4
  }) as [number, number, number]
  return 0.2126 * r + 0.7152 * g + 0.0722 * b
}

/**
 * WCAG contrast ratio between two colours.
 *
 * @param a First colour.
 * @param b Second colour.
 * @returns Ratio from 1 to 21.
 */
const ratio = (a: [number, number, number], b: [number, number, number]): number => {
  const [hi, lo] = [luminance(a), luminance(b)].sort((x, y) => y - x) as [number, number]
  return (hi + 0.05) / (lo + 0.05)
}

/** One template's footer in one mode: where its tokens live and which tokens it uses. */
interface Case {
  /** Label of the case. */
  name: string
  /** The token block. */
  source: string
  /** Page token. */
  page: string
  /** Footer token. */
  footer: string
  /** Resting link token. */
  rest: string
  /** Hover link token. */
  hover: string
}

/** Every template and mode. */
const CASES: Case[] = [
  { name: 'aurora dark', source: block(aurora, ':root,\n:root.dark {'), page: '--page', footer: '--footer', rest: '--mut', hover: '--ac1' },
  { name: 'aurora light', source: block(aurora, ':root.light {'), page: '--page', footer: '--footer', rest: '--mut', hover: '--ac1' },
  { name: 'nova dark', source: block(nova, '.tpl-nova {'), page: '--n-bg', footer: '--n-footer', rest: '--n-muted', hover: '--n-brand' },
  { name: 'nova light', source: block(nova, '.light .tpl-nova {'), page: '--n-bg', footer: '--n-footer', rest: '--n-muted', hover: '--n-brand' },
  { name: 'classic light', source: block(classic, ':root {'), page: '--classic-page', footer: '--classic-footer', rest: '--classic-muted', hover: '--classic-brand' },
  { name: 'classic dark', source: block(classic, '.dark {'), page: '--classic-page', footer: '--classic-footer', rest: '--classic-muted', hover: '--classic-brand' },
]

describe('footer tokens', () => {
  it.each(CASES)('$name: the footer is never darker than the page it ends, and clearly lighter in dark mode', (c) => {
    const footer = channels(c.source, c.footer)
    const page = channels(c.source, c.page)

    expect(luminance(footer)).toBeGreaterThanOrEqual(luminance(page))
    if (c.name.endsWith('dark')) {
      expect(ratio(footer, page)).toBeGreaterThanOrEqual(DARK_SEPARATION)
    }
  })

  it.each(CASES)('$name: resting and hover link colours pass AA on the footer', (c) => {
    const footer = channels(c.source, c.footer)

    expect(ratio(channels(c.source, c.rest), footer)).toBeGreaterThanOrEqual(AA)
    expect(ratio(channels(c.source, c.hover), footer)).toBeGreaterThanOrEqual(AA)
  })

  it('raises the dark footer above the page in every template', () => {
    for (const c of CASES.filter((entry) => entry.name.endsWith('dark'))) {
      expect(luminance(channels(c.source, c.footer))).toBeGreaterThan(luminance(channels(c.source, c.page)))
    }
  })
})

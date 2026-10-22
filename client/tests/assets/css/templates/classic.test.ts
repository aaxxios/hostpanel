/**
 * Guards the classic template's colour tokens: a complete light set on `:root`, a dark set
 * under `.dark`, and a page body that follows the colour-mode tokens instead of a fixed dark
 * colour (the regression behind issue 402, where classic stayed dark whatever the toggle said).
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

describe('classic tokens', () => {
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
    const value = (block: string): string => /--classic-page:\s*([^;]+);/.exec(block)![1]!.trim()

    expect(value(light)).not.toBe(value(dark))
  })

  it('paints the body from the colour-mode tokens, not a fixed dark colour', () => {
    const body = /(?:^|\n)body \{([^}]*)\}/.exec(global)![1]!

    expect(body).toContain('var(--page)')
    expect(body).toContain('var(--tx)')
    expect(body).not.toContain('#0a0a0f')
  })
})

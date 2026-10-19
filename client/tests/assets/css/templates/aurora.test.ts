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
